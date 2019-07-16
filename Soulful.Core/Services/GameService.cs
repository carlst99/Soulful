using LiteNetLib;
using LiteNetLib.Utils;
using Soulful.Core.Net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soulful.Core.Services
{
    public sealed class GameService : IGameService
    {
        public const int MAX_WHITE_CARDS = 5;

        private readonly INetServerService _server;
        private readonly ICardLoaderService _loader;
        private readonly Random rng;
        private CancellationTokenSource _stopToken;

        private Queue<int> _whiteCards;
        private Queue<int> _blackCards;
        private int _currentBlackCard;

        private int _czarPosition;

        public bool IsRunning { get; private set; }

        public GameService(INetServerService server, ICardLoaderService loader)
        {
            _server = server;
            _loader = loader;
            rng = new Random();
        }

        public void Start()
        {
            _whiteCards = new Queue<int>();
            _blackCards = new Queue<int>();
            _stopToken = new CancellationTokenSource();

            _server.AcceptingPlayers = false;
            _server.SendToAll(NetHelpers.GetKeyValue(GameKey.GameStart));
            SendWhiteCards(MAX_WHITE_CARDS);
            SendBlackCard();
            SendNextCzar();

            new Task(RunGame, _stopToken.Token, TaskCreationOptions.LongRunning).Start();

            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
            _stopToken.Cancel();
            _server.SendToAll(NetHelpers.GetKeyValue(GameKey.GameStop));
            _server.Stop();
        }

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

            // Send white cards to all peers
            NetDataWriter writer = new NetDataWriter();
            foreach (NetPeer player in _server.Players)
            {
                writer.Put((byte)GameKey.SendWhiteCards);
                for (int i = 0; i < count; i++)
                    writer.Put(_whiteCards.Dequeue());

                _server.Send(player, writer);
                writer.Reset();
            }
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

            // Send black card to all peers
            NetDataWriter writer = new NetDataWriter();
            foreach (NetPeer player in _server.Players)
            {
                writer.Put((byte)GameKey.SendBlackCard);
                writer.Put(_currentBlackCard);

                _server.Send(player, writer);
                writer.Reset();
            }
        }

        private void SendNextCzar()
        {
            if (_czarPosition == _server.Players.Count - 1)
                _czarPosition = 0;

            _server.Send(_server.Players[_czarPosition], NetHelpers.GetKeyValue(GameKey.InitiateCzar));
            _czarPosition++;
        }

        private string GetNextPackKey() => "Base";

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
