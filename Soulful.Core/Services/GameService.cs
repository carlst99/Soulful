using Soulful.Core.Net;

namespace Soulful.Core.Services
{
    public class GameService : IGameService
    {
        private readonly INetServerService _server;

        public GameService(INetServerService server)
        {
            _server = server;
        }

        public void Start()
        {
            _server.AcceptingPlayers = false;
            _server.SendToAll(NetConstants.GetKeyValue(GameKey.GameStart));
        }

        public void Stop()
        {
            _server.SendToAll(NetConstants.GetKeyValue(GameKey.GameStop));
            _server.Stop();
        }
    }
}
