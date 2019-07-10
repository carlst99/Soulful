using MvvmCross.Navigation;
using Soulful.Core.Net;

namespace Soulful.Core.ViewModels
{
    public class GameViewModel : Base.ViewModelBase<string>
    {
        private readonly IGameClientService _client;
        private string _playerName;

        public GameViewModel(IMvxNavigationService navigationService, IGameClientService client)
            : base(navigationService)
        {
            _client = client;
            _client.GameEvent += OnGameEvent;

            if (!_client.IsRunning)
                NavigationService.Navigate<HomeViewModel>();
        }

        private void OnGameEvent(object sender, GameKeyPackage e)
        {
            throw new System.NotImplementedException();
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
