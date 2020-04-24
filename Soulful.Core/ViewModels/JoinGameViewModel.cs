using IntraMessaging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Model;
using Soulful.Core.Net;
using Soulful.Core.Resources;
using System;
using System.Threading.Tasks;

namespace Soulful.Core.ViewModels
{
    public class JoinGameViewModel : Base.ViewModelBase<string>
    {
        #region Fields

        private readonly NetClientService _client;
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

        public IMvxCommand JoinGameCommand => new MvxAsyncCommand(JoinGame);
        public IMvxCommand NavigateBackCommand => new MvxCommand(NavigateBack);

        #endregion

        public JoinGameViewModel(IMvxNavigationService navigationService, NetClientService client, IIntraMessenger messenger)
            : base(navigationService)
        {
            _client = client;
            _messenger = messenger;
            _client.DisconnectedFromServer += OnDisconnected;
            _client.GameEvent += OnGameEvent;
        }

        private void OnDisconnected(object sender, NetKey e)
        {
            AttemptingConnection = false;
            ShowConfirmationLabel = false;

            string message;
            string title = null;
            switch (e)
            {
                case NetKey.Kicked:
                    title = AppStrings.DialogTitle_BlameServer;
                    message = AppStrings.DialogMessage_KickedFromServer;
                    break;
                case NetKey.ServerClosed:
                    title = AppStrings.DialogTitle_BlameServer;
                    message = AppStrings.DialogMessage_ServerClosed;
                    break;
                case NetKey.InvalidPin:
                    title = AppStrings.DialogTitle_InvalidPin;
                    message = AppStrings.DialogMessage_InvalidPin;
                    break;
                case NetKey.ServerFull:
                    message = AppStrings.DialogMessage_ServerFull;
                    break;
                case NetKey.ServerLimitChanged:
                    message = AppStrings.DialogMessage_ServerLimitChanged;
                    break;
                default:
                    title = AppStrings.DialogTitle_Disconnected;
                    message = AppStrings.DialogMessage_Disconnected;
                    break;
            }

            _messenger.Send(new DialogMessage
            {
                Title = title,
                Message = message
            });
        }

        private void OnGameEvent(object sender, GameKeyPackage e)
        {
            if (e.Key == GameKey.GameStart)
            {
                UnregisterEvents();
                NavigationService.Navigate<GameViewModel, Tuple<bool, string>>(new Tuple<bool, string>(false, _playerName));
            }
        }

        private async Task JoinGame()
        {
            AttemptingConnection = true;
            AttemptingConnection = ShowConfirmationLabel = await _client.Start(GamePin, _playerName).ConfigureAwait(false);
        }

        private void NavigateBack()
        {
            if (_client.IsConnected)
            {
                void callback(bool result)
                {
                    if (result)
                        UnsafeNavigateBack();
                }

                _messenger.Send(new DialogMessage
                {
                    Title = AppStrings.DialogTitle_LeavingGame,
                    Message = AppStrings.DialogMessage_ClientLeavingLobby,
                    OkayButtonContent = AppStrings.DialogButton_Yes,
                    CancelButtonContent = AppStrings.DialogButton_No,
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
            UnregisterEvents();
            _client.Stop();

            NavigationService.Navigate<HomeViewModel, string>(_playerName);
        }

        private void UnregisterEvents()
        {
            _client.DisconnectedFromServer -= OnDisconnected;
            _client.GameEvent -= OnGameEvent;
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
