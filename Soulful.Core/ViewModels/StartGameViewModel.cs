using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Net;
using System;
using System.Threading.Tasks;

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
            set
            {
                SetProperty(ref _maxPlayers, value);
                Server.ChangeMaxPlayers(value);
            }
        }

        public IGameServerService Server { get; }

#if DEBUG
        public bool CanStartGame => Server.Players.Count >= 1;
#else
        public bool CanStartGame => Server.Players.Count >= MIN_PLAYERS - 1;
#endif

        #endregion

        #region Commands

        public IMvxCommand RefreshGamePinCommand => new MvxCommand(GenerateGamePin);
        public IMvxCommand NavigateBackCommand => new MvxCommand(NavigateBack);
        public IMvxCommand StartGameCommand => new MvxCommand(StartGame);

        #endregion

        public StartGameViewModel(IMvxNavigationService navigationService, IGameServerService server)
            : base(navigationService)
        {
            Server = server;
            _maxPlayers = 20;

            GenerateGamePin();
            Server.Players.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(CanStartGame));
            Server.Start(MaxPlayers, GamePin);
        }

        private async void StartGame()
        {
            if (!CanStartGame)
                return;

            IGameClientService client = Mvx.IoCProvider.Resolve<IGameClientService>();
            client.Start(GamePin, _playerName);
            await Task.Run(async () =>
            {
                while (!client.IsRunning)
                    await Task.Delay(15).ConfigureAwait(false);
            }).ConfigureAwait(false);

            await NavigationService.Navigate<GameViewModel, string>(_playerName).ConfigureAwait(false);
        }

        private void NavigateBack()
        {
            if (Server.IsRunning)
                Server.Stop();
            NavigationService.Navigate<HomeViewModel>();
        }

        private void GenerateGamePin()
        {
            Random r = new Random();
            _gamePin = r.Next(100000, 999999);
            RaisePropertyChanged(nameof(GamePin));
            Server.ChangeConnectPin(GamePin);
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
