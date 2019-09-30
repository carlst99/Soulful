using IntraMessaging;
using LiteNetLib;
using LiteNetLib.Utils;
using Soulful.Core.Model;
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

        private Queue<int> _whiteCards;
        private Queue<int> _blackCards;
        private int _currentBlackCard;

        private int _czarPosition;
        private int _whitePackPosition;
        private int _blackPackPosition;
        private List<string> _packKeys;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating the state of this <see cref="GameService"/>
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets the full value of the current black card
        /// </summary>
        public Tuple<string, int> CurrentBlackCard => _loader.GetBlackCardAsync(_currentBlackCard).Result;

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

        public void Start(string playerName)
        {
            if (IsRunning)
                throw new InvalidOperationException("The game service is already running");

            // Hook up events
            _server.PlayerDisconnected += (_, e) => OnPlayerDisconnected(e);
            _server.GameEvent += OnGameEvent;

            // Initialise variables
            _whiteCards = new Queue<int>();
            _blackCards = new Queue<int>();
            _stopToken = new CancellationTokenSource();

            // Get pack keys if necessary
            if (_packKeys == null)
                _packKeys = _loader.Packs.Select(p => p.Key).ToList();

            // Set pack positions
            int randPackPosition = rng.Next(_loader.Packs.Count);
            _whitePackPosition = randPackPosition;
            _blackPackPosition = randPackPosition;

            // Prevent players from joining and add them to the local Player list
            _server.AcceptingPlayers = false;
            _players.AddRange(from NetPeer player in _server.Players
                              select new Player(player, (string)player.Tag));

            // Alert clients the game has started
            _server.SendToAll(NetHelpers.GetKeyValue(GameKey.GameStart));

            // Run the game
            new Task(RunGame, _stopToken.Token, TaskCreationOptions.LongRunning).Start();
            IsRunning = true;
        }

        public void Start(List<string> packKeys, string playerName)
        {
            if (IsRunning)
                throw new InvalidOperationException("The game service is already running");

            foreach (string key in packKeys)
            {
                if (_loader.Packs.Find(p => p.Key == key).Equals(default))
                    throw new ArgumentException("A provided key does not exist");
            }
            _packKeys = packKeys;
            Start(playerName);
        }

        public void Stop()
        {
            if (!IsRunning)
                throw new InvalidOperationException("The game service is already stopped");

            // Unregister events
            _server.PlayerDisconnected -= (_, e) => OnPlayerDisconnected(e);
            _server.GameEvent -= GameEvent;

            IsRunning = false;
            _stopToken.Cancel();
            _server.SendToAll(NetHelpers.GetKeyValue(GameKey.GameStop));
            _server.Stop();
            _players.Clear();
            GameStopped?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        private void OnGameEvent(object sender, GameKeyPackage e)
        {
            if (!TryGetPlayer(e.Peer, out Player p))
                return;

            switch (e.Key)
            {
                case GameKey.ClientSendWhiteCards:
                    while (!e.Data.EndOfData)
                    {
                        int card = e.Data.GetInt();
                        int unknownCount = 0;

                        // Add card, checking for invalid cards that have been sent
                        if (p.WhiteCards.Contains(card))
                            p.SelectedWhiteCards.Add(card);
                        else
                            p.SelectedWhiteCards.Add(p.WhiteCards[unknownCount++]);

                        // Limit cards to required amount
                        int cardCount = CurrentBlackCard.Item2;
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

        private void RunGame()
        {
            GameStage currentStage = GameStage.AwaitingPlayerReady;

            while (!_stopToken.IsCancellationRequested)
            {
                switch (currentStage)
                {
                    // Waiting for all players to ready up
                    case GameStage.AwaitingPlayerReady:
                        currentStage = GameStage.SendingRoundData;
                        foreach (Player p in _players)
                        {
                            if (!p.IsReady)
                            {
                                currentStage = GameStage.AwaitingPlayerReady;
                                break;
                            }
                        }
                        break;
                    // Preparing server and clients for the next round
                    case GameStage.SendingRoundData:
                        // Remove used white cards
                        foreach (Player p in _players)
                        {
                            foreach (int card in p.SelectedWhiteCards)
                                p.WhiteCards.Remove(card);
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

                        break;
                }

                _stopToken.Token.WaitHandle.WaitOne(NetHelpers.POLL_DELAY);
            }
        }

        private void SendWhiteCards()
        {
            void EnqueueWhiteCards()
            {
                // Initialise the list
                PackInfo pack = _loader.Packs.Find(p => p.Key == GetNextPackKey(true));
                List<int> whiteCards = new List<int>();
                for (int i = pack.WhiteStartRange; i < pack.WhiteStartRange + pack.WhiteCount; i++)
                    whiteCards.Add(i);

                // Remove cards that have already been distributed
                IEnumerable<int> existingCards = from player in _players
                                        from int value in player.WhiteCards
                                        select value;
                foreach (int element in existingCards)
                {
                    if (whiteCards.Contains(element))
                        whiteCards.Remove(element);
                }

                // Shuffle and enqueue the list
                Shuffle(whiteCards);
                foreach (int element in whiteCards)
                    _whiteCards.Enqueue(element);
            }

            // Generate a new set of white cards if needed
            // While loop used because some packs contain only black or only white cards
            while (_whiteCards.Count == 0)
                EnqueueWhiteCards();

            SendToAll(GameKey.SendWhiteCards, (w, p) =>
            {
                int cardCount = p.WhiteCards.Count;
                for (int i = 0; i < MAX_WHITE_CARDS - cardCount; i++)
                {
                    if (_whiteCards.Count == 0)
                        EnqueueWhiteCards();
                    int card = _whiteCards.Dequeue();
                    p.WhiteCards.Add(card);
                    w.Put(card);
                }
            });
        }

        private void SendBlackCard()
        {
            // Generate a new set of black cards if needed
            while (_blackCards.Count == 0)
            {
                // Initialise the list
                PackInfo pack = _loader.Packs.Find(p => p.Key == GetNextPackKey(false));
                List<int> blackCards = new List<int>();
                for (int i = pack.BlackStartRange; i < pack.BlackStartRange + pack.BlackCount; i++)
                    blackCards.Add(i);

                // Shuffle and enqueue the list
                Shuffle(blackCards);
                foreach (int element in blackCards)
                    _blackCards.Enqueue(element);
            }

            _currentBlackCard = _blackCards.Dequeue();
            NetDataWriter writer = new NetDataWriter();
            writer.Put(_currentBlackCard);
            SendToAll(GameKey.SendBlackCard, (w, _) => w.Put(_currentBlackCard));
        }

        private void SendNextCzar()
        {
            Player p = _players[_czarPosition];
            p.IsCzar = true;
            SendToPlayer(p, NetHelpers.GetKeyValue(GameKey.InitiateCzar));

            _czarPosition++;
            if (_czarPosition == _server.Players.Count)
                _czarPosition = 0;
        }

        /// <summary>
        /// Gets the next pack to use
        /// </summary>
        /// <returns>A pack key</returns>
        private string GetNextPackKey(bool whitePack)
        {
            if (_whitePackPosition == _loader.Packs.Count - 1)
                _whitePackPosition = 0;
            if (_blackPackPosition == _loader.Packs.Count - 1)
                _blackPackPosition = 0;

            return whitePack ? _packKeys[_whitePackPosition++] : _packKeys[_blackPackPosition++];
        }

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
        /// Sends data to both the server client and player clients
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

        #endregion

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
            if (_players[_czarPosition].Id == player.Id)
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
