using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using Soulful.Core.Model;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Soulful.Core.Net
{
    public sealed class NetClient : NetBase
    {
        #region Constants

        /// <summary>
        /// The time after which the client will stop trying to discover a server, after <see cref="Start(string, string)"/> is called
        /// </summary>
        public const int DISCOVERY_TIMEOUT = 3000;

        #endregion

        #region Fields

        private NetPeer _serverPeer;
        private string _pin;

        #endregion

        #region Properties

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

        #endregion

        public NetClient()
        {
            _listener.NetworkReceiveUnconnectedEvent += OnReceiveUnconnected;
            _listener.PeerDisconnectedEvent += OnPeerDisconnect;
        }

        public bool Start(string pin, string playerName)
        {
            Start();

            _pin = pin;
            PlayerName = playerName;

            // Request discovery
            NetDataWriter writer = new NetDataWriter();
            writer.Put(pin);
            _networker.SendDiscoveryRequest(writer, PORT);

            Log.Information("[Client]Client started");
            Log.Information("[Client]Attempting to discover server with pin {pin}", _pin);
            for (int i = 0; i < 10; i++)
            {
                Task.Delay(50).Wait();
                if (IsConnected)
                {
                    Log.Information("[Client]Connection delay (ms): {delay}", 50 * i);
                    return true;
                }
            }

            Log.Information("[Client]Failed to connect to server");
            Stop();
            return false;
        }

        public override void Stop()
        {
            IsConnected = false;
            if (IsRunning)
                base.Stop();
            Log.Information("[Client]Client stopped");
        }

        public void Send(NetDataWriter data)
        {
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("[Client]Cannot send data when the client is not running");

            if (!IsConnected)
                throw App.CreateError<InvalidOperationException>("[Client]Cannot send data when the client is not connected");

            _serverPeer.Send(data, D_METHOD);
        }

        protected override void OnReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            GameKey key = (GameKey)reader.PeekByte();
            if (key == GameKey.JoinedGame)
            {
                IsConnected = true;
                ConnectedToServer?.Invoke(this, EventArgs.Empty);
                reader.Recycle();
                Log.Information("[Client]Successfully connected to server at {endPoint}", peer.EndPoint);
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
                writer.Put(_pin);
                writer.Put(PlayerName);
                _serverPeer = _networker.Connect(remoteEndPoint, writer);
                Log.Information("[Client]Attempting to connect to server at {endPoint}", remoteEndPoint);
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

                Log.Information("[Client]Disconnected from server with reason {key}", key);
                DisconnectedFromServer?.Invoke(this, key);
            }
        }

        #endregion
    }
}
