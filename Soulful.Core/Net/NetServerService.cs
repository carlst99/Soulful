using LiteNetLib;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace Soulful.Core.Net
{
    public sealed class NetServerService : NetBase, INetServerService
    {
        public const DeliveryMethod DMethod = DeliveryMethod.ReliableOrdered;

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
        /// Gets a collection of players connected to the server
        /// </summary>
        public ObservableCollection<NetPeer> Players { get; }

        #endregion

        public NetServerService()
        {
            Players = new ObservableCollection<NetPeer>();

            _listener.ConnectionRequestEvent += OnConnectionRequested;
            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.PeerDisconnectedEvent += OnPeerDisconnected;
            _listener.NetworkReceiveUnconnectedEvent += OnReceiveUnconnected;

            _networker.DiscoveryEnabled = true;
        }

        public void Start(int maxPlayers, string pin)
        {
            Start();

            MaxPlayers = maxPlayers;
            Pin = pin;

            RunNetworkerTask(() => _networker.Start(NetConstants.PORT));
            Log.Information("Server started with pin {pin} and max connections {max}", pin, maxPlayers);
        }

        public override void Stop()
        {
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("Server is not running");

            foreach (NetPeer peer in RunNetworkerTask(() => _networker.ConnectedPeerList))
                peer.Disconnect(NetConstants.GetKeyValue(NetKey.DisconnectServerClosed));
            Players.Clear();

            base.Stop();
            Log.Information("Server stopped");
        }

        public void RunGame()
        {
            RunNetworkerTask(() => _networker.SendToAll(NetConstants.GetKeyValue(GameKey.GameStart), DMethod));
            // TODO - run game
        }

        public void Kick(int playerId)
        {
            NetPeer peer = Players.FirstOrDefault(p => p.Id == playerId);
            peer.Disconnect(NetConstants.GetKeyValue(NetKey.Kicked));
            Players.Remove(peer);
            Log.Information("Kicked player '{name}' at {endPoint}", peer.Tag, peer.EndPoint);
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
                    peer.Disconnect(NetConstants.GetKeyValue(NetKey.DisconnectLimitChanged));
            }
            else
            {
                RunNetworkerTask(() =>
                {
                    while (_networker.PeersCount > MaxPlayers)
                        _networker.ConnectedPeerList[_networker.PeersCount - 1].Disconnect(NetConstants.GetKeyValue(NetKey.DisconnectLimitChanged));
                });
            }
        }

        #region Peer connection handling

        private void OnReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryRequest)
            {
                string pin = reader.GetString();
                if (pin == Pin)
                    RunNetworkerTask(() => _networker.SendDiscoveryResponse(new byte[0], remoteEndPoint));
            }
        }

        private void OnConnectionRequested(ConnectionRequest request)
        {
            if (_networker.PeersCount < MaxPlayers)
            {
                string pin = request.Data.GetString();
                if (pin == Pin)
                {
                    NetPeer peer = request.Accept();
                    string userName = request.Data.GetString();
                    peer.Tag = userName;

                    Players.Add(peer);
                    Log.Information("Connection request from {endPoint} with username {userName} accepted", request.RemoteEndPoint, userName);
                }
                else
                {
                    request.Reject(NetConstants.GetKeyValue(NetKey.DisconnectInvalidPin));
                    Log.Information("Connection request from {endPoint} rejected due to invalid key", request.RemoteEndPoint);
                }
            }
            else
            {
                request.Reject(NetConstants.GetKeyValue(NetKey.DisconnectServerFull));
                Log.Information("Connection request from {endPoint} rejected as server full", request.RemoteEndPoint);
            }
        }

        private void OnPeerConnected(NetPeer peer)
        {
            peer.Send(NetConstants.GetKeyValue(GameKey.JoinedGame), DMethod);
            Log.Information("Alerting client that connection was successful");
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (Players.Contains(peer))
                Players.Remove(peer);

            Log.Information("Peer at {endPoint} disconnected", peer.EndPoint);
        }

        #endregion
    }
}
