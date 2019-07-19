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

        private CancellationTokenSource _stopToken;

        #endregion

        #region Game Fields

        private Queue<int> _whiteCards;
        private Queue<int> _blackCards;
        private int _currentBlackCard;

        private int _czarPosition;
        //private int _packPosition;
        private List<string> _packKeys;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating the state of this <see cref="GameService"/>
        /// </summary>
        public bool IsRunning { get; private set; }

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

        public GameService(INetServerService server, ICardLoaderService loader)
        {
            _server = server;
            _loader = loader;
            rng = new Random();
        }

        #region Start/Stop

        public async void Start()
        {
            if (IsRunning)
                throw new InvalidOperationException("The game service is already running");

            // Give clients time to navigate
            await Task.Delay(100).ConfigureAwait(false);

            _whiteCards = new Queue<int>();
            _blackCards = new Queue<int>();
            _stopToken = new CancellationTokenSource();
            if (_packKeys == null)
                _packKeys = _loader.Packs.Select(p => p.Key).ToList();

            _server.AcceptingPlayers = false;
            _server.SendToAll(NetHelpers.GetKeyValue(GameKey.GameStart));
            SendWhiteCards(MAX_WHITE_CARDS);
            SendBlackCard();
            SendNextCzar();

            new Task(RunGame, _stopToken.Token, TaskCreationOptions.LongRunning).Start();

            IsRunning = true;
        }

        public void Start(List<string> packKeys)
        {
            if (IsRunning)
                throw new InvalidOperationException("The game service is already running");

            foreach (string key in packKeys)
            {
                if (_loader.Packs.Find(p => p.Key == key).Equals(default))
                    throw new ArgumentException("A provided key does not exist");
            }
            _packKeys = packKeys;
            Start();
        }

        public void Stop()
        {
            if (!IsRunning)
                throw new InvalidOperationException("The game service is already stopped");

            IsRunning = false;
            _stopToken.Cancel();
            _server.SendToAll(NetHelpers.GetKeyValue(GameKey.GameStop));
            _server.Stop();
            GameStopped?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        private void RunGame()
        {
            while (!_stopToken.IsCancellationRequested)
            {
                _stopToken.Token.WaitHandle.WaitOne(NetHelpers.POLL_DELAY);
            }
        }

        private void SendWhiteCards(int count)
        {
            // Generate a new set of white cards if needed
            if (_whiteCards.Count <= 0)
            {
                // Initialise the list
                PackInfo pack = _loader.Packs.Find(p => p.Key == GetNextPackKey());
                List<int> whiteCards = new List<int>();
                for (int i = pack.WhiteStartRange; i < pack.WhiteStartRange + pack.WhiteCount; i++)
                    whiteCards.Add(i);

                // Shuffle and enqueue the list
                Shuffle(whiteCards);
                foreach (int element in whiteCards)
                    _whiteCards.Enqueue(element);
            }

            Send(GameKey.SendWhiteCards, (w) =>
            {
                for (int i = 0; i < count; i++)
                    w.Put(_whiteCards.Dequeue());
            });
        }

        private void SendBlackCard()
        {
            // Generate a new set of white cards if needed
            if (_blackCards.Count <= 0)
            {
                // Initialise the list
                PackInfo pack = _loader.Packs.Find(p => p.Key == GetNextPackKey());
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
            Send(GameKey.SendBlackCard, (w) => w.Put(_currentBlackCard));
        }

        private void SendNextCzar()
        {
            if (_czarPosition == _server.Players.Count - 1)
                _czarPosition = 0;

            if (_czarPosition == 0)
                SendToSelf(GameKey.InitiateCzar, null);
            else
                SendToPlayer(_server.Players[_czarPosition], NetHelpers.GetKeyValue(GameKey.InitiateCzar));
            _czarPosition++;
        }

        /// <summary>
        /// Gets the next pack to use
        /// </summary>
        /// <returns>A pack key</returns>
        private string GetNextPackKey()
        {
            //if (_packPosition == _loader.Packs.Count - 1)
            //    _packPosition = 0;

            //string key = _packKeys[_packPosition];
            //_packPosition++;
            //return key;
            return "Base";
        }

        #region Data distribution helper methods

        /// <summary>
        /// Invokes <see cref="GameEvent"/> with the required data and clears the writer
        /// </summary>
        /// <param name="key"></param>
        /// <param name="writer"></param>
        private void SendToSelf(GameKey key, NetDataWriter writer)
        {
            NetDataReader reader = null;
            if (writer != null)
                reader = new NetDataReader(writer.CopyData());

            GameKeyPackage package = new GameKeyPackage(key, reader, null);
            GameEvent?.Invoke(this, package);
            writer?.Reset();
        }

        /// <summary>
        /// Sends data to a player and clears the writer
        /// </summary>
        /// <param name="player"></param>
        /// <param name="writer"></param>
        private void SendToPlayer(NetPeer player, NetDataWriter writer)
        {
            _server.Send(player, writer);
            writer.Reset();
        }

        /// <summary>
        /// Sends data to both the server client and player clients
        /// </summary>
        /// <param name="key">The gamekey to send</param>
        /// <param name="dataFiller">An action to fill a <see cref="NetDataWriter"/> with data to send</param>
        private void Send(GameKey key, Action<NetDataWriter> dataFiller)
        {
            NetDataWriter writer = new NetDataWriter();

            dataFiller.Invoke(writer);
            GameKeyPackage package = new GameKeyPackage(key, new NetDataReader(writer.CopyData()), null);
            GameEvent?.Invoke(this, package);
            writer.Reset();

            foreach (NetPeer player in _server.Players)
            {
                writer.Put((byte)key);
                dataFiller.Invoke(writer);
                _server.Send(player, writer);
                writer.Reset();
            }
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
    }
}
