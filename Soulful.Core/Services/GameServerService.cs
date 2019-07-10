using LiteNetLib;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Soulful.Core.Services
{
    public sealed class GameServerService : IGameServerService
    {
        private readonly EventBasedNetListener _listener;
        private readonly NetManager _server;
        private Task _pollTask;

        public int MaxPlayers { get; private set; }
        public string Pin { get; private set; }
        public bool IsRunning => _server.IsRunning;

        public GameServerService()
        {
            _listener = new EventBasedNetListener();
            _listener.ConnectionRequestEvent += OnConnectionRequested;
            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.NetworkReceiveUnconnectedEvent += OnDiscoveryBroadcast;

            _server = new NetManager(_listener)
            {
                UnconnectedMessagesEnabled = true
            };
        }

        public void Start(int maxPlayers, string pin)
        {
            if (_server.IsRunning)
                throw App.CreateError<InvalidOperationException>("Server is already running");

            MaxPlayers = maxPlayers;
            Pin = pin;
            _server.Start(NetConstants.PORT);
            _pollTask = Task.Run(async () =>
            {
                if (_server.IsRunning)
                    _server.PollEvents();
                else
                    return;
                await Task.Delay(15).ConfigureAwait(false);
            });

            Log.Information("Server started");
        }

        public void Stop()
        {
            if (!_server.IsRunning)
                throw App.CreateError<InvalidOperationException>("Server is not running");

            foreach (NetPeer peer in _server.ConnectedPeerList)
                peer.Disconnect(NetConstants.GetKeyValue(NetKey.DisconnectServerClosed));
            _server.Stop();
            _pollTask.Wait();

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
                request.AcceptIfKey(Pin);
            else
                request.Reject(NetConstants.GetKeyValue(NetKey.DisconnectServerFull));
        }

        private void OnPeerConnected(NetPeer peer)
        {
            // TODO - add peer to peer list
        }

        private void OnDiscoveryBroadcast(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryRequest)
            {
                // TODO - send response saying you may connect
            }
        }
    }
}
