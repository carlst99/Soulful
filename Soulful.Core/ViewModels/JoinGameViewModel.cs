using IntraMessaging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Model;
using Soulful.Core.Net;
using System;

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

        private void OnDisconnected(object sender, LiteNetLib.DisconnectReason e)
        {
            AttemptingConnection = false;
            ShowConfirmationLabel = false;

            string message;
            string title;
            if (e == LiteNetLib.DisconnectReason.DisconnectPeerCalled)
            {
                title = "What've you done!?!";
                message = "Congratulations! It looks like you've been kicked.";
            }
            else if (e == LiteNetLib.DisconnectReason.RemoteConnectionClose)
            {
                title = "It was him!";
                message = "Looks like the host quit the game.";
            }
            else
            {
                title = "That... might've been us?";
                message = "Looks like you've been disconnected from the server, and we don't know why.";
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
                NavigationService.Navigate<GameViewModel, string>(_playerName);
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
            NavigationService.Navigate<HomeViewModel>();
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
