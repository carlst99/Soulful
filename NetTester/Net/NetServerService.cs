using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;

namespace NetTester.Net
{
    public sealed class NetServerService : NetBase
    {
        #region Properties

        /// <summary>
        /// Gets the maximum number of players that the server supports. Change this value using <see cref="ChangeMaxPlayers(int, bool)"/>
        /// </summary>
        public int MaxPlayers { get; private set; }

        /// <summary>
        /// Gets the pin used by the server to allow client connections. Change this using <see cref="ChangeConnectPin(string)"/>
        /// </summary>
        public string Pin { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the server is accepting new players
        /// </summary>
        public bool AcceptingPlayers { get; set; }

        /// <summary>
        /// Gets a collection of players connected to the server
        /// </summary>
        public List<NetPeer> Players => _networker.ConnectedPeerList;

        #endregion

        #region Events

        /// <summary>
        /// Asynchronously invoked when a player connects to the server
        /// </summary>
        public event EventHandler<NetPeer> PlayerConnected;

        /// <summary>
        /// Asynchronously invoked when a player disconnects from the server
        /// </summary>
        public event EventHandler<NetPeer> PlayerDisconnected;

        #endregion

        public NetServerService()
        {
            _listener.ConnectionRequestEvent += OnConnectionRequested;
            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.PeerDisconnectedEvent += OnPeerDisconnected;
            _listener.NetworkReceiveUnconnectedEvent += OnReceiveUnconnected;

            _networker.DiscoveryEnabled = true;
        }

        public void Start(int maxPlayers, string pin)
        {
            if (IsRunning)
                throw App.CreateError<InvalidOperationException>("[NetServer]Cannot start the server when it is already running");

            Start(PORT);

            MaxPlayers = maxPlayers;
            Pin = pin;
            AcceptingPlayers = true;

            Log.Information("[Server]Started with pin {pin} and max connections {max}", pin, maxPlayers);
        }

        public override void Stop()
        {
            AcceptingPlayers = false;
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("[Server]Cannot stop the server if it is not running");

            foreach (NetPeer peer in _networker.ConnectedPeerList)
                peer.Disconnect(NetHelpers.GetKeyValue(NetKey.ServerClosed));

            base.Stop();
            Log.Information("[Server]Server stopped");
        }

        public void Send(NetPeer peer, NetDataWriter data)
        {
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("[Server]Cannot send data when the server is not running");

            peer.Send(data, D_METHOD);
        }

        public void SendToAll(NetDataWriter data)
        {
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("[Server]Cannot send data when the server is not running");

            _networker.SendToAll(data, D_METHOD);
        }

        public void Kick(int playerId)
        {
            NetPeer peer = Players.Find(p => p.Id == playerId);
            peer.Disconnect(NetHelpers.GetKeyValue(NetKey.Kicked));
            PlayerDisconnected?.Invoke(this, peer);
            Log.Information("[Server]Kicked player '{name}' at {endPoint}", peer.Tag, peer.EndPoint);
        }

        /// <summary>
        /// Changes the pin required to connect to the server
        /// </summary>
        /// <param name="pin"></param>
        public void ChangeConnectPin(string pin) => Pin = pin;

        /// <summary>
        /// Changes the maximum number of players in the server. If the server has more players than the new maximum, the last players to connect will be disconnected
        /// </summary>
        /// <param name="maxPlayers">The new number of maximum players</param>
        /// <param name="disconnectAll">If true, all players are disconnected, regardless of how many are currently connected</param>
        public void ChangeMaxPlayers(int maxPlayers, bool disconnectAll = false)
        {
            MaxPlayers = maxPlayers;

            if (disconnectAll)
            {
                foreach (NetPeer peer in _networker.ConnectedPeerList)
                {
                    peer.Disconnect(NetHelpers.GetKeyValue(NetKey.ServerLimitChanged));
                    PlayerDisconnected?.Invoke(this, peer);
                }
            }
            else
            {
                while (_networker.PeersCount > MaxPlayers)
                {
                    NetPeer peer = _networker.ConnectedPeerList[_networker.PeersCount - 1];
                    peer.Disconnect(NetHelpers.GetKeyValue(NetKey.ServerLimitChanged));
                    PlayerDisconnected?.Invoke(this, peer);
                }
            }
        }

        #region Peer connection handling

        private void OnReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryRequest)
            {
                string pin = reader.GetString();
                if (pin == Pin)
                    _networker.SendDiscoveryResponse(Array.Empty<byte>(), remoteEndPoint);
            }
        }

        private void OnConnectionRequested(ConnectionRequest request)
        {
            if (_networker.PeersCount < MaxPlayers && AcceptingPlayers)
            {
                string pin = request.Data.GetString();
                if (pin == Pin)
                {
                    NetPeer peer = request.Accept();
                    string userName = request.Data.GetString();
                    peer.Tag = userName;
                    Log.Information("[Server]Connection request from {endPoint} with username {userName} accepted", request.RemoteEndPoint, userName);
                }
                else
                {
                    request.Reject(NetHelpers.GetKeyValue(NetKey.InvalidPin));
                    Log.Information("[Server]Connection request from {endPoint} rejected due to invalid key", request.RemoteEndPoint);
                }
            }
            else
            {
                request.Reject(NetHelpers.GetKeyValue(NetKey.ServerFull));
                Log.Information("[Server]Connection request from {endPoint} rejected as server full", request.RemoteEndPoint);
            }
        }

        private void OnPeerConnected(NetPeer peer)
        {
            PlayerConnected?.Invoke(this, peer);
            Send(peer, NetHelpers.GetKeyValue(GameKey.JoinedGame));
            Log.Information("[Server]Alerting client that connection was successful");
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            PlayerDisconnected?.Invoke(this, peer);
            Log.Information("[Server]Peer at {endPoint} disconnected", peer.EndPoint);
        }

        #endregion
    }
}
