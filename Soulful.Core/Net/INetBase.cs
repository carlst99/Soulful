using Soulful.Core.Model;
using System;

namespace Soulful.Core.Net
{
    public interface INetBase
    {
        bool IsRunning { get; }

        event EventHandler<GameKeyPackage> GameEvent;

        void Stop();
    }
}
