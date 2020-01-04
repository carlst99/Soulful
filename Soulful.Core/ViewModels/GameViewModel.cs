using IntraMessaging;
using LiteNetLib.Utils;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Model;
using Soulful.Core.Net;
using Soulful.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Soulful.Core.ViewModels
{
    public class GameViewModel : Base.ViewModelBase<bool>
    {
        #region Fields

        private readonly INetClientService _client;
        private readonly IGameService _gameService;
        private readonly IIntraMessenger _messenger;

        private bool _isServer;
        private ObservableCollection<LeaderboardEntry> _leaderboard;
        private ObservableCollection<int> _whiteCards;
        private ObservableCollection<int> _selectedWhiteCards;
        private int _blackCard;
        private bool _czarMode;
        private string _sendButtonText;
        private bool _canSendCards;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current white cards
        /// </summary>
        public ObservableCollection<int> WhiteCards
        {
            get => _whiteCards;
            set => SetProperty(ref _whiteCards, value);
        }

        /// <summary>
        /// Gets or sets the currently selected white cards
        /// </summary>
        public ObservableCollection<int> SelectedWhiteCards
        {
            get => _selectedWhiteCards;
            set => SetProperty(ref _selectedWhiteCards, value);
        }

        /// <summary>
        /// Gets or sets the current black card
        /// </summary>
        public int BlackCard
        {
            get => _blackCard;
            set => SetProperty(ref _blackCard, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the client is acting as czar
        /// </summary>
        public bool CzarMode
        {
            get => _czarMode;
            set => SetProperty(ref _czarMode, value);
        }

        /// <summary>
        /// Gets or sets the text shown in the card send button
        /// </summary>
        public string SendButtonText
        {
            get => _sendButtonText;
            set => SetProperty(ref _sendButtonText, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the client can send white cards
        /// </summary>
        public bool CanSendCards
        {
            get => _canSendCards;
            set => SetProperty(ref _canSendCards, value);
        }

        /// <summary>
        /// Gets or sets the leaderboard
        /// </summary>
        public ObservableCollection<LeaderboardEntry> Leaderboard
        {
            get => _leaderboard;
            set => SetProperty(ref _leaderboard, value);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Invoked when the client wishes to leave the game
        /// </summary>
        public IMvxCommand NavigateBackCommand => new MvxCommand(NavigateBack);

        /// <summary>
        /// Invoked when the client wishes to send card selections to the server
        /// </summary>
        public IMvxCommand PickCardsCommand => new MvxCommand(PickCards);

        #endregion

        public GameViewModel(IMvxNavigationService navigationService, INetClientService client, IIntraMessenger messenger, IGameService gameService)
            : base(navigationService)
        {
            _client = client;
            _gameService = gameService;
            _messenger = messenger;

            Leaderboard = new ObservableCollection<LeaderboardEntry>();
            WhiteCards = new ObservableCollection<int>();
            SendButtonText = this["Command_PlayerPickCards"];
            CanSendCards = true;
        }

        public override Task Initialize()
        {
            _client.GameEvent += (_, e) => EOMT(() => OnGameEvent(e));
            _client.DisconnectedFromServer += OnDisconnected;
            _client.Send(NetHelpers.GetKeyValue(GameKey.ClientReady));

            return base.Initialize();
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
                    CzarMode = true;
                    SendButtonText = this["Command_CzarPickCards"];
                    break;
                case GameKey.UpdatingLeaderboard:
                    while (!e.Data.EndOfData)
                    {
                        int id = e.Data.GetInt();
                        int score = e.Data.GetInt();

                        if (Leaderboard.Any(l => l.PlayerId == id))
                            Leaderboard.First(l => l.PlayerId == id).Score = score;
                    }
                    DoLeaderboardManipulation();
                    break;
                case GameKey.SendingInitialLeaderboard:
                    while (!e.Data.EndOfData)
                        Leaderboard.Add(new LeaderboardEntry(e.Data.GetInt(), e.Data.GetString()));
                    break;
            }
        }

        /// <summary>
        /// Prepares the <see cref="GameViewModel"/>
        /// </summary>
        /// <param name="parameter">Indicates whether the game service should be started</param>
        public override void Prepare(bool parameter)
        {
            if (!_client.IsRunning)
                NavigationService.Navigate<HomeViewModel>();

            if (parameter)
            {
                _isServer = true;
                _gameService.Start();
                _gameService.GameStopped += (_, __) => UnsafeNavigateBack();
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

            if (_gameService.IsRunning)
                _gameService.Stop();

            if (_client.IsRunning)
                _client.Stop();

            await NavigationService.Navigate<HomeViewModel>().ConfigureAwait(false);
        }

        private void PickCards()
        {
            if (CzarMode)
            {
                
            } else
            {
                NetDataWriter writer = NetHelpers.GetKeyValue(GameKey.ClientSendWhiteCards);
                foreach (int card in SelectedWhiteCards)
                    writer.Put(card);
                _client.Send(writer);
                writer.Reset();
            }

            CanSendCards = false;
        }

        /// <summary>
        /// Finds the top and bottom players in the leaderboard and marks them as so
        /// </summary>
        private void DoLeaderboardManipulation()
        {
            int highestScore = Leaderboard.Max(l => l.Score);
            int lowestScore = Leaderboard.Min(l => l.Score);

            foreach (LeaderboardEntry element in Leaderboard)
                element.Reset();

            foreach (LeaderboardEntry element in Leaderboard.Where(l => l.Score == highestScore))
                element.IsTop = true;

            foreach (LeaderboardEntry element in Leaderboard.Where(l => l.Score == lowestScore))
                element.IsBottom = true;
        }

        private void OnDisconnected(object sender, NetKey e)
        {
            UnregisterEvents();
            if (_isServer)
                return;

            string message;
            string title;
            switch (e)
            {
                case NetKey.Kicked:
                    title = "What've you done!?!";
                    message = "Congratulations! It looks like you've been kicked.";
                    break;
                case NetKey.ServerClosed:
                    title = "It wasn't me!";
                    message = "Looks like the host quit the game.";
                    break;
                default:
                    title = "That... might've been me?";
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
                _gameService.GameStopped -= (_, __) => UnsafeNavigateBack();

            if (_client.IsRunning)
            {
                _client.GameEvent -= (_, e) => EOMT(() => OnGameEvent(e));
                _client.DisconnectedFromServer -= OnDisconnected;
            }
        }
    }
}
