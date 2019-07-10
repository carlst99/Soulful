using MvvmCross.Commands;
using MvvmCross.Navigation;
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
            set => SetProperty(ref _maxPlayers, value);
        }

        #endregion

        #region Commands

        public IMvxCommand RefreshGamePinCommand => new MvxCommand(GenerateGamePin);
        public IMvxCommand NavigateBackCommand => new MvxCommand(NavigateBack);

        #endregion

        public StartGameViewModel(IMvxNavigationService navigationService)
            : base(navigationService)
        {
            GenerateGamePin();
            _maxPlayers = 20;
        }

        private void NavigateBack()
        {
            // TODO close server etc.
            NavigationService.Navigate<HomeViewModel>();
        }

        private void GenerateGamePin()
        {
            Random r = new Random();
            _gamePin = r.Next(100000, 999999);
            RaisePropertyChanged(nameof(GamePin));
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
