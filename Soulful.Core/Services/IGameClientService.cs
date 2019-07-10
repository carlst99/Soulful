using System;

namespace Soulful.Core.Services
{
    public interface IGameClientService
    {
        bool IsRunning { get; }
        event EventHandler ConnectedToServer;

        void Start(string pin, string userName);
        void Stop();
    }
}
