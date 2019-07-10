using System;
using System.Collections.Generic;
using System.Text;

namespace Soulful.Core.Services
{
    public interface IGameServerService
    {
        void Start(int maxPlayers, string key);
        void ChangeConnectPin(string key);
        void ChangeMaxPlayers(int maxPlayers, bool kickLast = true);
    }
}
