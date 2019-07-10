using LiteNetLib;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Soulful.Core.Services
{
    public sealed class GameServerService : IGameServerService
    {
        private readonly EventBasedNetListener _listener;
        private readonly NetManager _server;
        private readonly Dictionary<IPEndPoint, string> _temporaryConnections;
        private Task _pollTask;
        private CancellationTokenSource _cancelPollToken;

        public int MaxPlayers { get; private set; }
        public string Pin { get; private set; }
        public bool IsRunning => _server.IsRunning;
        public ObservableCollection<NetPeer> Players { get; private set; }

        public GameServerService()
        {
            _temporaryConnections = new Dictionary<IPEndPoint, string>();
            Players = new ObservableCollection<NetPeer>();

            _listener = new EventBasedNetListener();
            _listener.ConnectionRequestEvent += OnConnectionRequested;
            _listener.PeerConnectedEvent += OnPeerConnected;
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

            Log.Information("Server started");
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
            } else
            {
                while (_server.PeersCount > MaxPlayers)
                    _server.ConnectedPeerList[_server.PeersCount - 1].Disconnect(NetConstants.GetKeyValue(NetKey.DisconnectLimitChanged));
            }
        }

        /// <summary>
        /// Invoked when a client requests to connect
        /// </summary>
        /// <param name="request"></param>
        private void OnConnectionRequested(ConnectionRequest request)
        {
            if (_server.PeersCount < MaxPlayers)
            {
                string pin = request.Data.GetString();
                if (pin == Pin)
                {
                    request.Accept();
                    string userName = request.Data.GetString();
                    _temporaryConnections.Add(request.RemoteEndPoint, userName);
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
            if (_temporaryConnections.ContainsKey(peer.EndPoint))
            {
                peer.Tag = _temporaryConnections[peer.EndPoint];
                _temporaryConnections.Remove(peer.EndPoint);

                Players.Add(peer);
                Log.Information("Peer completed connection from {endPoint}", peer.EndPoint);
            } else
            {
                peer.Disconnect(NetConstants.GetKeyValue(NetKey.DisconnectUnknownError));
                Log.Error("Peer connected with no request: {endPoint}", peer.EndPoint);
            }
        }

        private void OnReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryRequest)
            {
                string pin = reader.GetString();
                if (pin == Pin)
                    _server.SendDiscoveryResponse(new byte[0], remoteEndPoint);
            }
        }
    }
}
