using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.ObjectModel;

namespace Soulful.Core.Net
{
    public interface INetServerService
    {
        bool IsRunning { get; }
        bool AcceptingPlayers { get; set; }
        ObservableCollection<NetPeer> Players { get; }

        void Start(int maxPlayers, string pin);
        void Stop();
        void ChangeConnectPin(string pin);
        void ChangeMaxPlayers(int maxPlayers, bool disconnectAll = false);
        void SendToAll(NetDataWriter data);
        void Send(NetPeer peer, NetDataWriter data);
        void Kick(int playerId);
    }
}
