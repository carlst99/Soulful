namespace Soulful.Core.Services
{
    public interface IGameServerService
    {
        bool IsRunning { get; }

        void Start(int maxPlayers, string key);
        void Stop();
        void ChangeConnectPin(string key);
        void ChangeMaxPlayers(int maxPlayers, bool kickLast = true);
    }
}
