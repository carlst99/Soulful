using IntraMessaging;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Model;
using Soulful.Core.Net;
using System;
using System.Collections.ObjectModel;

namespace Soulful.Core.ViewModels
{
    public class GameViewModel : Base.ViewModelBase<string>
    {
        #region Fields

        private readonly INetClientService _client;
        private readonly IIntraMessenger _messenger;

        private string _playerName;

        private ObservableCollection<int> _whiteCards;
        private int _blackCard;

        #endregion

        #region Properties

        public ObservableCollection<int> WhiteCards
        {
            get => _whiteCards;
            set => SetProperty(ref _whiteCards, value);
        }

        public int BlackCard
        {
            get => _blackCard;
            set => SetProperty(ref _blackCard, value);
        }

        #endregion

        #region Commands

        public IMvxCommand NavigateBackCommand => new MvxCommand(NavigateBack);

        #endregion

        public GameViewModel(IMvxNavigationService navigationService, INetClientService client, IIntraMessenger messenger)
            : base(navigationService)
        {
            _client = client;
            _messenger = messenger;

            _client.GameEvent += OnGameEvent;
            _client.DisconnectedFromServer += OnDisconnected;

            _whiteCards = new ObservableCollection<int>();

            if (!_client.IsRunning)
                NavigationService.Navigate<HomeViewModel>();
        }

        private void OnGameEvent(object sender, GameKeyPackage e)
        {
            switch (e.Key)
            {
                case GameKey.SendWhiteCards:
                    while (!e.Data.EndOfData)
                        EOMT(() => WhiteCards.Add(e.Data.GetInt()));
                    break;
                case GameKey.SendBlackCard:
                    EOMT(() => BlackCard = e.Data.GetInt());
                    break;
                case GameKey.InitiateCzar:
                    _whiteCards.Clear();
                    break;
            }
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }

        private void NavigateBack()
        {
            void callback(DialogMessage.Button b)
            {
                if (b == DialogMessage.Button.Yes)
                    UnsafeNavigateBack();
            }

            _messenger.Send(new DialogMessage
            {
                Title = "Sore loser",
                Content = "Are you sure you want to quit?",
                Buttons = DialogMessage.Button.Yes | DialogMessage.Button.No,
                Callback = callback
            });
        }

        private async void UnsafeNavigateBack()
        {
            _client.Stop();

            INetServerService server = Mvx.IoCProvider.Resolve<INetServerService>();
            if (server.IsRunning)
            {
                while (_client.IsRunning)
                {
                    await System.Threading.Tasks.Task.Delay(NetHelpers.POLL_DELAY).ConfigureAwait(false);
                }
                server.Stop();
            }

            await NavigationService.Navigate<HomeViewModel>().ConfigureAwait(false);
        }

        private void OnDisconnected(object sender, LiteNetLib.DisconnectReason e)
        {
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

            NavigationService.Navigate<HomeViewModel>();
        }

        /// <summary>
        /// Provides a syntatic shortcut <see cref="AsyncDispatcher.ExecuteOnMainThreadAsync"/>
        /// </summary>
        /// <param name="action">The action to execute</param>
        private void EOMT(Action action) => AsyncDispatcher.ExecuteOnMainThreadAsync(action);
    }
}
