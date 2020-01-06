using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace Soulful.Core.Net
{
    public interface INetServerService : INetBase
    {
        bool AcceptingPlayers { get; set; }
        List<NetPeer> Players { get; }
        event EventHandler<NetPeer> PlayerConnected;
        event EventHandler<NetPeer> PlayerDisconnected;

        void Start(int maxPlayers, string pin);
        void ChangeConnectPin(string pin);
        void ChangeMaxPlayers(int maxPlayers, bool disconnectAll = false);
        void SendToAll(NetDataWriter data);
        void Send(NetPeer peer, NetDataWriter data);
        void Kick(int playerId);
    }
}
