using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using Soulful.Core.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace Soulful.Core.Net
{
    public sealed class NetServerService : NetBase, INetServerService
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
        public ObservableCollection<NetPeer> Players { get; }

        #endregion

        public event EventHandler<NetPeer> PlayerDisconnected;

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
            RunNetworkerTask(() => _networker.Start(PORT));

            MaxPlayers = maxPlayers;
            Pin = pin;
            AcceptingPlayers = true;

            Log.Information("Server started with pin {pin} and max connections {max}", pin, maxPlayers);
        }

        public override void Stop()
        {
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("Server is not running");

            AcceptingPlayers = false;
            Players.Clear();
            foreach (NetPeer peer in RunNetworkerTask(() => _networker.ConnectedPeerList))
                RunNetworkerTask(() => peer.Disconnect(NetHelpers.GetKeyValue(NetKey.ServerClosed)));

            base.Stop();
            Log.Information("Server stopped");
        }

        public void Send(NetPeer peer, NetDataWriter data)
        {
            if (!IsRunning)
                throw new InvalidOperationException("Server is not running");

            RunNetworkerTask(() => peer.Send(data, D_METHOD));
        }

        public void SendToAll(NetDataWriter data)
        {
            if (!IsRunning)
                throw new InvalidOperationException("Server is not running");

            RunNetworkerTask(() => _networker.SendToAll(data, D_METHOD));
        }

        public void Kick(int playerId)
        {
            NetPeer peer = Players.FirstOrDefault(p => p.Id == playerId);
            RunNetworkerTask(() => peer.Disconnect(NetHelpers.GetKeyValue(NetKey.Kicked)));
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
                    RunNetworkerTask(() => peer.Disconnect(NetHelpers.GetKeyValue(NetKey.ServerLimitChanged)));
            }
            else
            {
                RunNetworkerTask(() =>
                {
                    while (_networker.PeersCount > MaxPlayers)
                    {
                        RunNetworkerTask(() => _networker.ConnectedPeerList[_networker.PeersCount - 1].Disconnect(NetHelpers.GetKeyValue(NetKey.ServerLimitChanged)));
                    }
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
            if (_networker.PeersCount < MaxPlayers && AcceptingPlayers)
            {
                string pin = request.Data.GetString();
                if (pin == Pin)
                {
                    NetPeer peer = RunNetworkerTask(() => request.Accept());
                    string userName = request.Data.GetString();
                    peer.Tag = userName;

                    Players.Add(peer);
                    Log.Information("Connection request from {endPoint} with username {userName} accepted", request.RemoteEndPoint, userName);
                }
                else
                {
                    RunNetworkerTask(() => request.Reject(NetHelpers.GetKeyValue(NetKey.InvalidPin)));
                    Log.Information("Connection request from {endPoint} rejected due to invalid key", request.RemoteEndPoint);
                }
            }
            else
            {
                RunNetworkerTask(() => request.Reject(NetHelpers.GetKeyValue(NetKey.ServerFull)));
                Log.Information("Connection request from {endPoint} rejected as server full", request.RemoteEndPoint);
            }
        }

        private void OnPeerConnected(NetPeer peer)
        {
            Send(peer, NetHelpers.GetKeyValue(GameKey.JoinedGame));
            Log.Information("Alerting client that connection was successful");
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (Players.Contains(peer))
            {
                Players.Remove(peer);
                PlayerDisconnected?.Invoke(this, peer);
            }

            Log.Information("Peer at {endPoint} disconnected", peer.EndPoint);
        }

        #endregion
    }
}
