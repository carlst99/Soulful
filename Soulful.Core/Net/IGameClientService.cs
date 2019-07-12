using LiteNetLib;
using System;

namespace Soulful.Core.Net
{
    public interface IGameClientService
    {
        bool IsRunning { get; }
        bool IsConnected { get; }

        event EventHandler ConnectedToServer;
        event EventHandler<DisconnectReason> DisconnectedFromServer;
        event EventHandler ConnectionFailed;
        event EventHandler<GameKeyPackage> GameEvent;

        void Start(string pin, string playerName);
        void Stop();
    }
}
