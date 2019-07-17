using LiteNetLib;
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
    }
}
