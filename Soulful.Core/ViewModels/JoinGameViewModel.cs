using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Net;

namespace Soulful.Core.ViewModels
{
    public class JoinGameViewModel : Base.ViewModelBase<string>
    {
        #region Fields

        private readonly INetClientService _client;

        private string _gamePin;
        private string _playerName;
        private bool _showConfirmationLabel;

        private bool _attemptingConnection;

        #endregion

        #region Properties

        public string GamePin
        {
            get => _gamePin;
            set
            {
                SetProperty(ref _gamePin, value);
                RaisePropertyChanged(nameof(CanJoinGame));
            }
        }

        public bool AttemptingConnection
        {
            get => _attemptingConnection;
            set
            {
                SetProperty(ref _attemptingConnection, value);
                RaisePropertyChanged(nameof(CanJoinGame));
            }
        }

        public bool ShowConfirmationLabel
        {
            get => _showConfirmationLabel;
            set => SetProperty(ref _showConfirmationLabel, value);
        }

        public bool CanJoinGame => !AttemptingConnection && GamePin?.Length == 6;

        #endregion

        #region Commands

        public IMvxCommand JoinGameCommand => new MvxCommand(JoinGame);
        public IMvxCommand NavigateBackCommand => new MvxCommand(NavigateBack);

        #endregion

        public JoinGameViewModel(IMvxNavigationService navigationService, INetClientService client)
            : base(navigationService)
        {
            _client = client;
            _client.ConnectedToServer += (s, e) => ShowConfirmationLabel = true;
            _client.DisconnectedFromServer += OnDisconnected;
            _client.ConnectionFailed += (s, e) => AttemptingConnection = false;
            _client.GameEvent += OnGameEvent;
        }

        private void OnDisconnected(object sender, LiteNetLib.DisconnectReason e)
        {
            AttemptingConnection = false;
            ShowConfirmationLabel = false;
        }

        private void OnGameEvent(object sender, GameKeyPackage e)
        {
            if (e.Key == GameKey.GameStart)
                NavigationService.Navigate<GameViewModel, string>(_playerName);
        }

        private void JoinGame()
        {
            _client.Start(GamePin, _playerName);
            AttemptingConnection = true;
        }

        private void NavigateBack()
        {
            if (_client.IsRunning)
                _client.Stop();
            NavigationService.Navigate<HomeViewModel>();
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
