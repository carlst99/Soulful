using LiteNetLib.Utils;
using System;

namespace Soulful.Core.Net
{
    public interface INetClientService : INetBase
    {
        bool IsConnected { get; }

        event EventHandler ConnectedToServer;
        event EventHandler<NetKey> DisconnectedFromServer;
        event EventHandler ConnectionFailed;

        void Start(string pin, string playerName);
        void Send(NetDataWriter data);
        void ConnectLocal(string pin, string playerName);
    }
}
