using IntraMessaging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Model;
using Soulful.Core.Net;
using Soulful.Core.Services;
using System;
using System.Collections.ObjectModel;

namespace Soulful.Core.ViewModels
{
    public class GameViewModel : Base.ViewModelBase<bool>
    {
        #region Fields

        private readonly INetClientService _client;
        private readonly IGameService _gameService;
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

        public GameViewModel(IMvxNavigationService navigationService, INetClientService client, IIntraMessenger messenger, IGameService gameService)
            : base(navigationService)
        {
            _client = client;
            _gameService = gameService;
            _messenger = messenger;
            _whiteCards = new ObservableCollection<int>();

            if (_client.IsRunning)
            {
                _client.GameEvent += (_, e) => EOMT(() => OnGameEvent(e));
                _client.DisconnectedFromServer += OnDisconnected;
            }
        }

        /// <summary>
        /// Note that this method is called on the main thread from the event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameEvent(GameKeyPackage e)
        {
            switch (e.Key)
            {
                case GameKey.SendWhiteCards:
                    while (!e.Data.EndOfData)
                        WhiteCards.Add(e.Data.GetInt());
                    break;
                case GameKey.SendBlackCard:
                    BlackCard = e.Data.GetInt();
                    break;
                case GameKey.InitiateCzar:
                    _whiteCards.Clear();
                    break;
            }
        }

        public override void Prepare(bool parameter)
        {
            if (parameter)
            {
                _gameService.GameEvent += (_, e) => EOMT(() => OnGameEvent(e));
                _gameService.GameStopped += (_, __) => UnsafeNavigateBack();
                _gameService.Start();
            } else if (!_client.IsRunning)
            {
                NavigationService.Navigate<HomeViewModel>();
            }
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
            UnregisterEvents();

            if (_client.IsRunning)
                _client.Stop();

            if (_gameService.IsRunning)
                _gameService.Stop();

            await NavigationService.Navigate<HomeViewModel>().ConfigureAwait(false);
        }

        private void OnDisconnected(object sender, NetKey e)
        {
            UnregisterEvents();
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

            NavigationService.Navigate<HomeViewModel>();
        }

        /// <summary>
        /// Provides a syntatic shortcut to <see cref="AsyncDispatcher.ExecuteOnMainThreadAsync"/>
        /// </summary>
        /// <param name="action">The action to execute</param>
        private void EOMT(Action action) => AsyncDispatcher.ExecuteOnMainThreadAsync(action);

        private void UnregisterEvents()
        {
            if (_gameService.IsRunning)
            {
                _gameService.GameEvent -= (_, e) => EOMT(() => OnGameEvent(e));
                _gameService.GameStopped -= (_, __) => UnsafeNavigateBack();
            } else
            {
                _client.GameEvent -= (_, e) => EOMT(() => OnGameEvent(e));
                _client.DisconnectedFromServer -= OnDisconnected;
            }
        }
    }
}
