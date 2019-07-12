using LiteNetLib;
using System.Collections.ObjectModel;

namespace Soulful.Core.Net
{
    public interface INetServerService
    {
        bool IsRunning { get; }
        ObservableCollection<NetPeer> Players { get; }

        void Start(int maxPlayers, string pin);
        void Stop();
        void ChangeConnectPin(string pin);
        void ChangeMaxPlayers(int maxPlayers, bool kickLast = true);
        void RunGame();
        void Kick(int playerId);
    }
}
