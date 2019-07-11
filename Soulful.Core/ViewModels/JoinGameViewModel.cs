using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Net;
using System.Threading.Tasks;

namespace Soulful.Core.ViewModels
{
    public class JoinGameViewModel : Base.ViewModelBase<string>
    {
        #region Fields

        private readonly IGameClientService _client;

        private string _gamePin;
        private string _playerName;
        private bool _showConfirmationLabel;

        private bool _attemptingConnection;
        private bool _attemptFinished;

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

        public JoinGameViewModel(IMvxNavigationService navigationService, IGameClientService client)
            : base(navigationService)
        {
            _client = client;
            _client.ConnectedToServer += OnConnectionSucceeded;
            _client.GameEvent += OnGameEvent;
        }

        private void OnConnectionSucceeded(object sender, System.EventArgs e)
        {
            _attemptFinished = true;
            ShowConfirmationLabel = true;
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
            _attemptFinished = false;
            // TODO - need awaiter? need to continue in same context?
            Task.Run(async () =>
            {
                await Task.Delay(3000).ConfigureAwait(false);
                if (!_attemptFinished)
                {
                    AttemptingConnection = false;
                    _attemptFinished = true;
                    _client.Stop();
                }
            }).ConfigureAwait(false);
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
