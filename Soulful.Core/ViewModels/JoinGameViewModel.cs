using IntraMessaging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Model;
using Soulful.Core.Net;

namespace Soulful.Core.ViewModels
{
    public class JoinGameViewModel : Base.ViewModelBase<string>
    {
        #region Fields

        private readonly INetClientService _client;
        private readonly IIntraMessenger _messenger;

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

        public JoinGameViewModel(IMvxNavigationService navigationService, INetClientService client, IIntraMessenger messenger)
            : base(navigationService)
        {
            _client = client;
            _messenger = messenger;
            _client.ConnectedToServer += (s, e) => ShowConfirmationLabel = true;
            _client.DisconnectedFromServer += OnDisconnected;
            _client.ConnectionFailed += (s, e) => AttemptingConnection = false;
            _client.GameEvent += OnGameEvent;
        }

        private void OnDisconnected(object sender, NetKey e)
        {
            UnregisterEvents();
            AttemptingConnection = false;
            ShowConfirmationLabel = false;

            string message;
            string title;
            switch (e)
            {
                case NetKey.Kicked:
                    title = "What've you done!?!";
                    message = "Congratulations! It looks like you've been kicked.";
                    break;
                case NetKey.ServerClosed:
                    title = "It was him!";
                    message = "Looks like the host quit the game.";
                    break;
                case NetKey.InvalidPin:
                    title = "Hacker alert";
                    message = "We don't know how you've done it... but you've connected to the server with the wrong pin. Sorry bud, try again!";
                    break;
                case NetKey.ServerFull:
                    title = "Server full";
                    message = "Sorry bud, but this server's full. Try asking the host to increase the number of max players.";
                    break;
                case NetKey.ServerLimitChanged:
                    title = "Unlucky!";
                    message = "The server host changed the number of maximum players, and you didn't make the cut. If you've got a problem, now would be a good time to take it up with the host.";
                    break;
                default:
                    title = "That... might've been us?";
                    message = "Looks like you've been disconnected from the server, and we don't know why.";
                    break;
            }

            _messenger.Send(new DialogMessage
            {
                Title = title,
                Content = message,
                Buttons = DialogMessage.Button.Ok
            });
        }

        private void OnGameEvent(object sender, GameKeyPackage e)
        {
            if (e.Key == GameKey.GameStart)
            {
                UnregisterEvents();
                NavigationService.Navigate<GameViewModel, bool>(false);
            }
        }

        private void JoinGame()
        {
            _client.Start(GamePin, _playerName);
            AttemptingConnection = true;
        }

        private void NavigateBack()
        {
            if (_client.IsConnected)
            {
                void callback(DialogMessage.Button b)
                {
                    if (b == DialogMessage.Button.Yes)
                        UnsafeNavigateBack();
                }

                _messenger.Send(new DialogMessage
                {
                    Title = "WTF?!?",
                    Content = "The game hasn't even started yet! Are you sure you want to quit?",
                    Buttons = DialogMessage.Button.Yes | DialogMessage.Button.No,
                    Callback = callback
                });
            }
            else
            {
                UnsafeNavigateBack();
            }
        }

        private void UnsafeNavigateBack()
        {
            if (_client.IsRunning)
                _client.Stop();

            UnregisterEvents();
            NavigationService.Navigate<HomeViewModel>();
        }

        private void UnregisterEvents()
        {
            _client.ConnectedToServer -= (s, a) => ShowConfirmationLabel = true;
            _client.DisconnectedFromServer -= OnDisconnected;
            _client.ConnectionFailed -= (s, a) => AttemptingConnection = false;
            _client.GameEvent -= OnGameEvent;
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
