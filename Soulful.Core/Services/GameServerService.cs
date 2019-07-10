using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Soulful.Core.Services
{
    public sealed class GameServerService : IGameServerService
    {
        private readonly EventBasedNetListener _listener;
        private readonly NetManager _server;

        public int MaxPlayers { get; private set; }
        public string Pin { get; private set; }

        public GameServerService()
        {
            _listener = new EventBasedNetListener();
            _listener.ConnectionRequestEvent += OnConnectionRequested;

            _server = new NetManager(_listener);
        }

        public void Start(int maxPlayers, string pin)
        {
            MaxPlayers = maxPlayers;
            Pin = pin;
            _server.Start(NetConstants.PORT);
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
                    peer.Disconnect(NetConstants.GetKeyValue(NetKey.DisconnectServerFull));
            } else
            {
                while (_server.PeersCount > MaxPlayers)
                    _server.ConnectedPeerList[_server.PeersCount - 1].Disconnect(NetConstants.GetKeyValue(NetKey.DisconnectServerFull));
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
    }
}
