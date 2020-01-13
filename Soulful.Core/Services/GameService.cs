using IntraMessaging;
using LiteNetLib;
using LiteNetLib.Utils;
using Soulful.Core.Model;
using Soulful.Core.Model.Cards;
using Soulful.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Soulful.Core.Services
{
    public sealed class GameService : IGameService
    {
        public const int MAX_WHITE_CARDS = 5;

        #region Service Fields

        private readonly INetServerService _server;
        private readonly ICardLoaderService _loader;
        private readonly Random rng;
        private readonly IIntraMessenger _messenger;

        private CancellationTokenSource _stopToken;

        #endregion

        #region Game Fields

        private readonly List<Player> _players;

        private CyclicCardQueue<WhiteCard> _whiteCards;
        private CyclicCardQueue<BlackCard> _blackCards;

        private int _czarPosition;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating the state of this <see cref="GameService"/>
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets the full value of the current black card
        /// </summary>
        public BlackCard CurrentBlackCard { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when a game event occurs
        /// </summary>
        /// <remarks>This event should match any client side game events</remarks>
        public event EventHandler<GameKeyPackage> GameEvent;

        /// <summary>
        /// Invoked when the game is stopped
        /// </summary>
        public event EventHandler GameStopped;

        #endregion

        public GameService(INetServerService server, ICardLoaderService loader, IIntraMessenger messenger)
        {
            _server = server;
            _loader = loader;
            _messenger = messenger;
            rng = new Random();
            _players = new List<Player>();
        }

        #region Start/Stop

        public void Start(List<Pack> packs = null)
        {
            if (IsRunning)
                throw App.CreateError<InvalidOperationException>("[GameService]Cannot start the game service when it is already running");

            // Initialise card queues
            if (packs == null)
                packs = _loader.Packs;
            SetupCardQueues(packs);

            // Hook up events
            _server.PlayerDisconnected += (_, e) => OnPlayerDisconnected(e);
            _server.GameEvent += OnGameEvent;

            // Initialise variables
            _stopToken = new CancellationTokenSource();

            // Prevent players from joining and add them to the local Player list
            _server.AcceptingPlayers = false;
            _players.AddRange(from NetPeer player in _server.Players
                              select new Player(player, (string)player.Tag));

            // Alert clients the game has started
            _server.SendToAll(NetHelpers.GetKeyValue(GameKey.GameStart));

            // Run the game
            new Task(RunGame, TaskCreationOptions.LongRunning).Start();
            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning)
                throw new InvalidOperationException("[GameService]Cannot stop the game service if it is not running");

            // Unregister events
            _server.PlayerDisconnected -= (_, e) => OnPlayerDisconnected(e);
            _server.GameEvent -= GameEvent;

            IsRunning = false;
            _stopToken.Cancel();
            _server.SendToAll(NetHelpers.GetKeyValue(GameKey.GameStop));
            _server.Stop();
            _players.Clear();
            _czarPosition = 0;
            GameStopped?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        private async void OnGameEvent(object sender, GameKeyPackage e)
        {
            if (!TryGetPlayer(e.Peer, out Player p))
                return;

            switch (e.Key)
            {
                case GameKey.ClientSendWhiteCards:
                    while (!e.Data.EndOfData)
                    {
                        WhiteCard card = await _loader.GetWhiteCardAsync(e.Data.GetInt()).ConfigureAwait(true);
                        int unknownCount = 0;

                        // Add card, checking for invalid cards that have been sent
                        if (p.WhiteCards.Contains(card))
                            p.SelectedWhiteCards.Add(card);
                        else
                            p.SelectedWhiteCards.Add(p.WhiteCards[unknownCount++]);

                        // Limit cards to required amount
                        int cardCount = CurrentBlackCard.NumPicks;
                        while (p.SelectedWhiteCards.Count < cardCount)
                            p.SelectedWhiteCards.Add(p.WhiteCards[unknownCount++]);
                        while (p.SelectedWhiteCards.Count > cardCount)
                            p.SelectedWhiteCards.RemoveAt(p.SelectedWhiteCards.Count - 1);
                    }
                    break;
                case GameKey.ClientReady:
                    p.IsReady = true;
                    break;
            }
        }

        private void RunGame()
        {
            GameStage currentStage = GameStage.AwaitingPlayerReady;

            while (!_stopToken.IsCancellationRequested)
            {
                switch (currentStage)
                {
                    // Waiting for all players to ready up
                    case GameStage.AwaitingPlayerReady:
                        currentStage = GameStage.SendingInitialLeaderboard;
                        foreach (Player p in _players)
                        {
                            if (!p.IsReady)
                            {
                                currentStage = GameStage.AwaitingPlayerReady;
                                break;
                            }
                        }
                        break;
                    case GameStage.SendingInitialLeaderboard:
                        SendInitialLeaderboard();
                        currentStage = GameStage.SendingRoundData;
                        break;
                    // Preparing server and clients for the next round
                    case GameStage.SendingRoundData:
                        // Remove used white cards
                        foreach (Player p in _players)
                        {
                            foreach (WhiteCard card in p.SelectedWhiteCards)
                            {
                                p.WhiteCards.Remove(card);
                                _whiteCards.Enqueue(card);
                            }
                        }

                        // Clear selected cards
                        foreach (Player p in _players)
                            p.SelectedWhiteCards.Clear();

                        // Send new cards
                        SendWhiteCards();
                        SendBlackCard();
                        SendNextCzar();

                        currentStage = GameStage.AwaitingCardSelections;
                        break;
                    // Waiting for clients to send their card selections to the server
                    case GameStage.AwaitingCardSelections:
                        currentStage = GameStage.SendingCardsToCzar;
                        foreach (Player p in _players)
                        {
                            if (p.SelectedWhiteCards.Count == 0 && !p.IsCzar)
                            {
                                currentStage = GameStage.AwaitingCardSelections;
                                break;
                            }
                        }
                        break;
                    // Sending selections to czar
                    case GameStage.SendingCardsToCzar:
                        Player czar = _players[_czarPosition];
                        czar.IsCzar = false;
                        // Todo send card selections to czar
                        currentStage = GameStage.AwaitingCzarPick;
                        break;
                    case GameStage.AwaitingCzarPick:
                        // TODO await czar pick
                        // Verify sender is czar first
                        // Timeout/number verify for network issues
                        currentStage = GameStage.UpdatingLeaderboard;
                        break;
                    // Sends an updated leaderboard to all players
                    case GameStage.UpdatingLeaderboard:
                        SendUpdatedLeaderboard();
                        currentStage = GameStage.SendingRoundData;
                        break;
                }

                _stopToken.Token.WaitHandle.WaitOne(NetHelpers.POLL_DELAY);
            }
        }

        #region Send methods

        private void SendWhiteCards()
        {
            SendToAll(GameKey.SendWhiteCards, (w, p) =>
            {
                int cardCount = p.WhiteCards.Count;
                for (int i = 0; i < MAX_WHITE_CARDS - cardCount; i++)
                {
                    WhiteCard card = _whiteCards.Dequeue();
                    p.WhiteCards.Add(card);
                    w.Put(card.Id);
                }
            });
        }

        private void SendBlackCard()
        {
            CurrentBlackCard = _blackCards.Dequeue(false);
            SendToAll(GameKey.SendBlackCard, (w, _) => w.Put(CurrentBlackCard.Id));
        }

        private void SendNextCzar()
        {
            _czarPosition++;
            if (_czarPosition == _players.Count)
                _czarPosition = 0;

            Player p = _players[_czarPosition];
            p.IsCzar = true;
            SendToPlayer(p, NetHelpers.GetKeyValue(GameKey.InitiateCzar));
        }

        private void SendInitialLeaderboard()
        {
            // Fix duplicate names
            Dictionary<string, int> nameCountPairs = new Dictionary<string, int>();
            foreach (Player player in _players)
            {
                if (!nameCountPairs.ContainsKey(player.Name))
                {
                    nameCountPairs.Add(player.Name, 0);
                }
                else
                {
                    // Increment the duplicate count
                    nameCountPairs[player.Name]++;
                    player.Name += nameCountPairs[player.Name].ToString();
                }
            }
            nameCountPairs.Clear();

            _players.OrderBy(k => k.Name);

            // Send leaderboard to each player
            SendToAll(GameKey.SendingInitialLeaderboard, (w, p) =>
            {
                foreach (Player player in _players)
                {
                    string name = player.Name;
                    if (p.Id == player.Id)
                        name += " " + Resources.AppStrings.Leaderboard_You;

                    w.Put(player.Id);
                    w.Put(player.Name);
                }
            });
        }

        private void SendUpdatedLeaderboard()
        {
            NetDataWriter w = NetHelpers.GetKeyValue(GameKey.UpdatingLeaderboard);
            foreach (Player p in _players)
            {
                if (p.NeedsLeaderboardUpdate)
                {
                    w.Put(p.Id);
                    w.Put(p.Score);
                }
            }
            SendToAll(w);
        }

        #endregion

        #region Data distribution helper methods

        /// <summary>
        /// Sends data to a player and clears the writer
        /// </summary>
        /// <param name="player"></param>
        /// <param name="writer"></param>
        private void SendToPlayer(Player player, NetDataWriter writer)
        {
            _server.Send(player.Peer, writer);
            writer.Reset();
        }

        /// <summary>
        /// Fills a <see cref="NetDataWriter"/> using the specified action and sends this to each player
        /// </summary>
        /// <param name="dataFiller">An action to fill a <see cref="NetDataWriter"/> with data to send</param>
        private void SendToAll(GameKey key, Action<NetDataWriter, Player> dataFiller)
        {
            NetDataWriter writer = null;
            foreach (Player p in _players)
            {
                writer = NetHelpers.GetKeyValue(key);
                dataFiller.Invoke(writer, p);
                SendToPlayer(p, writer);
            }
            writer?.Reset();
        }

        /// <summary>
        /// Sends a data package to all players
        /// </summary>
        /// <param name="writer">The data package to send</param>
        private void SendToAll(NetDataWriter writer)
        {
            foreach (Player p in _players)
                _server.Send(p.Peer, writer);
            writer.Reset();
        }

        #endregion

        /// <summary>
        /// Attempts to find a player with the provided <see cref="NetPeer"/>
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool TryGetPlayer(NetPeer peer, out Player player)
        {
            // Make sure that this player is in our list
            if (!_players.Any(p => p.Id == peer.Id))
            {
                player = null;
                return false;
            }

            // Add the white cards to the player's selected cards list
            player = _players.First(p => p.Id == peer.Id);
            return true;
        }

        /// <summary>
        /// Sets up the circular card queues
        /// </summary>
        /// <param name="packKeys"></param>
        private void SetupCardQueues(List<Pack> packs)
        {
            List<BlackCard> blackCards = new List<BlackCard>();
            List<WhiteCard> whiteCards = new List<WhiteCard>();

            foreach (Pack pack in packs)
            {
                blackCards.AddRange(pack.BlackCards);
                whiteCards.AddRange(pack.WhiteCards);
            }

            Shuffle(blackCards);
            Shuffle(whiteCards);
            _blackCards = new CyclicCardQueue<BlackCard>(blackCards.ToArray());
            _whiteCards = new CyclicCardQueue<WhiteCard>(whiteCards.ToArray());
        }

        /// <summary>
        /// Shuffles a list using the Fisher-Yates algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        private void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void OnPlayerDisconnected(NetPeer player)
        {
            if (_players.Count > 0 && _players[_czarPosition].Id == player.Id)
            {
                // TODO cancel round, send next czar
            }
            _players.RemoveAt(_players.FindIndex(p => p.Id == player.Id));

            if (_players.Count <= 1)
            {
                Stop();
                _messenger.Send(new DialogMessage
                {
                    Title = "All players quit",
                    Content = "Guess nobody wants to play with you, huh?",
                    Buttons = DialogMessage.Button.No
                });
            }
        }
    }
}
