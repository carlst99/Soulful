using LiteNetLib;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Soulful.Core.Net
{
    public sealed class GameServerService : IGameServerService
    {
        public const DeliveryMethod DMethod = DeliveryMethod.ReliableOrdered;

        #region Fields

        private readonly EventBasedNetListener _listener;
        private readonly NetManager _server;
        private Task _pollTask;
        private CancellationTokenSource _cancelPollToken;

        #endregion

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
        /// Gets a value indicating whether or not the server is running
        /// </summary>
        public bool IsRunning => _server.IsRunning;

        /// <summary>
        /// Gets a collection of players connected to the server
        /// </summary>
        public ObservableCollection<NetPeer> Players { get; }

        #endregion

        public GameServerService()
        {
            Players = new ObservableCollection<NetPeer>();

            _listener = new EventBasedNetListener();
            _listener.ConnectionRequestEvent += OnConnectionRequested;
            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.PeerDisconnectedEvent += OnPeerDisconnected;
            _listener.NetworkReceiveUnconnectedEvent += OnReceiveUnconnected;

            _server = new NetManager(_listener)
            {
                DiscoveryEnabled = true
            };
        }

        public void Start(int maxPlayers, string pin)
        {
            if (IsRunning)
                throw App.CreateError<InvalidOperationException>("Server is already running");

            _cancelPollToken = new CancellationTokenSource();
            MaxPlayers = maxPlayers;
            Pin = pin;

            _server.Start(NetConstants.PORT);
            _pollTask = Task.Run(async () =>
            {
                while (!_cancelPollToken.IsCancellationRequested)
                {
                    _server.PollEvents();
                    await Task.Delay(15).ConfigureAwait(false);
                }
            }, _cancelPollToken.Token);

            Log.Information("Server started with pin {pin} and max connections {max}", pin, maxPlayers);
        }

        public void Stop()
        {
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("Server is not running");

            _cancelPollToken.Cancel();
            _pollTask.Wait();

            foreach (NetPeer peer in _server.ConnectedPeerList)
                peer.Disconnect(NetConstants.GetKeyValue(NetKey.DisconnectServerClosed));
            _server.Stop();

            Log.Information("Server stopped");
        }

        public void RunGame()
        {
            _server.SendToAll(NetConstants.GetKeyValue(GameKey.GameStart), DMethod);
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
                foreach (NetPeer peer in _server.ConnectedPeerList)
                    peer.Disconnect(NetConstants.GetKeyValue(NetKey.DisconnectLimitChanged));
            }
            else
            {
                while (_server.PeersCount > MaxPlayers)
                    _server.ConnectedPeerList[_server.PeersCount - 1].Disconnect(NetConstants.GetKeyValue(NetKey.DisconnectLimitChanged));
            }
        }

        #region Peer connection handling

        private void OnReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryRequest)
            {
                string pin = reader.GetString();
                if (pin == Pin)
                    _server.SendDiscoveryResponse(new byte[0], remoteEndPoint);
            }
        }

        private void OnConnectionRequested(ConnectionRequest request)
        {
            if (_server.PeersCount < MaxPlayers)
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
