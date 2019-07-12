using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Soulful.Core.Net
{
    public sealed class GameClientService : IGameClientService
    {
        /// <summary>
        /// The time after which the client will stop trying to discover a server, after <see cref="Start(string, string)"/> is called
        /// </summary>
        public const int DISCOVERY_TIMEOUT = 3000;

        #region Fields

        private readonly EventBasedNetListener _listener;
        private readonly NetManager _client;
        private Task _pollTask;
        private CancellationTokenSource _cancelPollToken;
        private NetPeer _serverPeer;

        #endregion

        #region Properties

        public string Pin { get; private set; }
        public string UserName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the client is running
        /// </summary>
        public bool IsRunning => _client.IsRunning;

        /// <summary>
        /// Gets a value indicating whether or not the client is connected to the server
        /// </summary>
        public bool IsConnected { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the client connects to a server
        /// </summary>
        public event EventHandler ConnectedToServer;

        /// <summary>
        /// Invoked when the client is disconnected
        /// </summary>
        public event EventHandler<DisconnectReason> DisconnectedFromServer;

        /// <summary>
        /// Invoked when a game-related event occurs
        /// </summary>
        public event EventHandler<GameKeyPackage> GameEvent;

        /// <summary>
        /// Invoked when the server fails to connect to a server
        /// </summary>
        public event EventHandler ConnectionFailed;

        #endregion

        public GameClientService()
        {
            _listener = new EventBasedNetListener();
            _listener.NetworkReceiveUnconnectedEvent += OnReceiveUnconnected;
            _listener.NetworkReceiveEvent += OnReceive;
            _listener.PeerDisconnectedEvent += OnPeerDisconnect;

            _client = new NetManager(_listener);
        }

        public void Start(string pin, string userName)
        {
            if (IsRunning)
                throw App.CreateError<InvalidOperationException>("Client is already running");

            // Setup variables
            _cancelPollToken = new CancellationTokenSource();
            Pin = pin;
            UserName = userName;

            // Start
            _client.Start();
            _pollTask = Task.Run(() =>
            {
                while (!_cancelPollToken.IsCancellationRequested)
                {
                    _client.PollEvents();
                    Task.Delay(NetConstants.POLL_DELAY).Wait();
                }
            });

            // Request discovery
            NetDataWriter writer = new NetDataWriter();
            writer.Put(pin);
            _client.SendDiscoveryRequest(writer, NetConstants.PORT);

            // Stop on timeout
            Task.Run(() =>
            {
                Task.Delay(DISCOVERY_TIMEOUT).Wait();
                if (!IsConnected && IsRunning)
                {
                    Log.Information("Client failed to connect to server");
                    Stop();
                    ConnectionFailed?.Invoke(this, EventArgs.Empty);
                }
            });

            Log.Information("Client started");
            Log.Information("Client attempting to discover server with pin {pin}", Pin);
        }

        public void Stop()
        {
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("Client is not running");

            IsConnected = false;
            _cancelPollToken.Cancel();
            _pollTask.Wait();
            _client.Stop(true);

            Log.Information("Client stopped");
        }

        private void OnReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            GameKey key = (GameKey)reader.GetByte();
            if (key == GameKey.JoinedGame)
            {
                IsConnected = true;
                ConnectedToServer?.Invoke(this, EventArgs.Empty);
                Log.Information("Client successfully connected to server at {endPoint}", peer.EndPoint);
            }
            else
            {
                GameKeyPackage package = new GameKeyPackage(key, reader, peer);
                GameEvent?.Invoke(this, package);
            }

            reader.Recycle();
        }

        #region Server connection handling

        private void OnReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryResponse)
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put(Pin);
                writer.Put(UserName);
                _serverPeer = _client.Connect(remoteEndPoint, writer);
                Log.Information("Client attempting to connect to server at {endPoint}", remoteEndPoint);
            }
            reader.Recycle();
        }

        private void OnPeerDisconnect(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (peer.Id == _serverPeer.Id)
            {
                Log.Information("Client disconnected from server with reason {reason}", disconnectInfo.Reason);
                Stop();
                DisconnectedFromServer?.Invoke(this, disconnectInfo.Reason);
            }
        }

        #endregion
    }
}
