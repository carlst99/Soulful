using System;

namespace Soulful.Core.Net
{
    public interface IGameClientService
    {
        bool IsRunning { get; }
        event EventHandler ConnectedToServer;
        event EventHandler<GameKeyPackage> GameEvent;

        void Start(string pin, string playerName);
        void Stop();
    }
}
