namespace Soulful.Core.Services
{
    public interface IGameService
    {
        bool IsRunning { get; }

        void Start();
        void Stop();
    }
}
