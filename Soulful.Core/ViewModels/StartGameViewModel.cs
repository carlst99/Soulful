using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Services;
using System;

namespace Soulful.Core.ViewModels
{
    public class StartGameViewModel : Base.ViewModelBase<string>
    {
        #region Constants

        public const double MIN_PLAYERS = 3;
        public const double MAX_PLAYERS = 100;

        #endregion

        #region Fields

        private readonly IGameServerService _server;

        private int _gamePin;
        private int _maxPlayers;
        private string _playerName;

        #endregion

        #region Properties

        public string GamePin
        {
            get => _gamePin.ToString("000000");
        }

        public int MaxPlayers
        {
            get => _maxPlayers;
            set
            {
                SetProperty(ref _maxPlayers, value);
                _server.ChangeMaxPlayers(value);
            }
        }

        #endregion

        #region Commands

        public IMvxCommand RefreshGamePinCommand => new MvxCommand(GenerateGamePin);
        public IMvxCommand NavigateBackCommand => new MvxCommand(NavigateBack);

        #endregion

        public StartGameViewModel(IMvxNavigationService navigationService, IGameServerService server)
            : base(navigationService)
        {
            _server = server;
            _maxPlayers = 20;

            GenerateGamePin();
            _server.Start(MaxPlayers, GamePin);
        }

        private void NavigateBack()
        {
            if (_server.IsRunning)
                _server.Stop();
            NavigationService.Navigate<HomeViewModel>();
        }

        private void GenerateGamePin()
        {
            Random r = new Random();
            _gamePin = r.Next(100000, 999999);
            RaisePropertyChanged(nameof(GamePin));
            _server.ChangeConnectPin(GamePin);
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
