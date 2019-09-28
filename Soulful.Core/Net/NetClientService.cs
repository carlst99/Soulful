using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using Soulful.Core.Model;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Soulful.Core.Net
{
    public sealed class NetClientService : NetBase, INetClientService
    {
        #region Constants

        /// <summary>
        /// The time after which the client will stop trying to discover a server, after <see cref="Start(string, string)"/> is called
        /// </summary>
        public const int DISCOVERY_TIMEOUT = 3000;

        #endregion

        #region Fields

        private NetPeer _serverPeer;

        #endregion

        #region Properties

        public string Pin { get; private set; }
        public string PlayerName { get; private set; }

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
        public event EventHandler<NetKey> DisconnectedFromServer;

        /// <summary>
        /// Invoked when the server fails to connect to a server
        /// </summary>
        public event EventHandler ConnectionFailed;

        #endregion

        public NetClientService()
        {
            _listener.NetworkReceiveUnconnectedEvent += OnReceiveUnconnected;
            //_listener.NetworkReceiveEvent += OnReceive;
            _listener.PeerDisconnectedEvent += OnPeerDisconnect;
        }

        public void Start(string pin, string playerName)
        {
            Start();
            RunNetworkerTask(() => _networker.Start());

            Pin = pin;
            PlayerName = playerName;

            // Request discovery
            NetDataWriter writer = new NetDataWriter();
            writer.Put(pin);
            RunNetworkerTask(() => _networker.SendDiscoveryRequest(writer, PORT));

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

        public override void Stop()
        {
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("Client is not running");

            IsConnected = false;
            base.Stop();
            Log.Information("Client stopped");
        }

        public void Send(NetDataWriter data)
        {
            RunNetworkerTask(() => _serverPeer.Send(data, D_METHOD));
        }

        protected override void OnReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            GameKey key = (GameKey)reader.PeekByte();
            if (key == GameKey.JoinedGame)
            {
                IsConnected = true;
                ConnectedToServer?.Invoke(this, EventArgs.Empty);
                reader.Recycle();
                Log.Information("Client successfully connected to server at {endPoint}", peer.EndPoint);
            }
            else
            {
                base.OnReceive(peer, reader, deliveryMethod);
            }
        }

        #region Server connection handling

        private void OnReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryResponse)
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put(Pin);
                writer.Put(PlayerName);
                _serverPeer = RunNetworkerTask(() => _networker.Connect(remoteEndPoint, writer));
                Log.Information("Client attempting to connect to server at {endPoint}", remoteEndPoint);
            }
            reader.Recycle();
        }

        private void OnPeerDisconnect(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (peer.Id == _serverPeer.Id)
            {
                Stop();

                NetKey key;
                if (disconnectInfo.AdditionalData.AvailableBytes > 0)
                    key = (NetKey)disconnectInfo.AdditionalData.GetByte();
                else
                    key = NetKey.DisconnectUnknownError;

                Log.Information("Client disconnected from server with reason {key}", key);
                DisconnectedFromServer?.Invoke(this, key);
            }
        }

        #endregion
    }
}
