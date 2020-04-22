using IntraMessaging;
using LiteNetLib.Utils;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Model;
using Soulful.Core.Model.Cards;
using Soulful.Core.Net;
using Soulful.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Soulful.Core.ViewModels
{
    public class GameViewModel : Base.ViewModelBase<Tuple<bool, string>>
    {
        #region Fields

        private readonly NetClientService _client;
        private readonly IGameService _gameService;
        private readonly IIntraMessenger _messenger;
        private readonly ICardLoaderService _cardLoader;

        private string _playerName;
        private bool _isServer;
        private ObservableCollection<LeaderboardEntry> _leaderboard;
        private ObservableCollection<WhiteCard> _whiteCards;
        private ObservableCollection<WhiteCard> _selectedWhiteCards;
        private BlackCard _blackCard;
        private bool _czarMode;
        private string _sendButtonText;
        private bool _canSendCards;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current white cards
        /// </summary>
        public ObservableCollection<WhiteCard> WhiteCards
        {
            get => _whiteCards;
            set => SetProperty(ref _whiteCards, value);
        }

        /// <summary>
        /// Gets or sets the currently selected white cards
        /// </summary>
        public ObservableCollection<WhiteCard> SelectedWhiteCards
        {
            get => _selectedWhiteCards;
            set => SetProperty(ref _selectedWhiteCards, value);
        }

        /// <summary>
        /// Gets or sets the current black card
        /// </summary>
        public BlackCard BlackCard
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

        public GameViewModel(
            IMvxNavigationService navigationService,
            NetClientService client,
            IIntraMessenger messenger,
            IGameService gameService,
            ICardLoaderService cardLoader)
            : base(navigationService)
        {
            _client = client;
            _gameService = gameService;
            _messenger = messenger;
            _cardLoader = cardLoader;

            Leaderboard = new ObservableCollection<LeaderboardEntry>();
            WhiteCards = new ObservableCollection<WhiteCard>();
            SendButtonText = this["Command_PlayerPickCards"];
            CanSendCards = true;
        }

        public override Task Initialize()
        {
            _client.GameEvent += (_, e) => EOMT(() => OnGameEvent(e));
            _client.DisconnectedFromServer += (_, e) => EOMT(() => OnDisconnected(e));
            _client.Send(NetHelpers.GetKeyValue(GameKey.ClientReady));

            return base.Initialize();
        }

        /// <summary>
        /// Note that this method is called on the main thread from the event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnGameEvent(GameKeyPackage e)
        {
            switch (e.Key)
            {
                case GameKey.SendWhiteCards:
                    while (!e.Data.EndOfData)
                        WhiteCards.Add(await _cardLoader.GetWhiteCardAsync(e.Data.GetInt()).ConfigureAwait(false));
                    break;
                case GameKey.SendBlackCard:
                    BlackCard = await _cardLoader.GetBlackCardAsync(e.Data.GetInt()).ConfigureAwait(false);
                    break;
                case GameKey.InitiateCzar:
                    CzarMode = true;
                    SendButtonText = Resources.AppStrings.Command_CzarPickCards;
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
                    while (e.Data.AvailableBytes > 0)
                        Leaderboard.Add(new LeaderboardEntry(e.Data.GetInt(), e.Data.GetString()));
                    break;
            }
        }

        /// <summary>
        /// Prepares the <see cref="GameViewModel"/>
        /// </summary>
        /// <param name="parameter">Indicates whether the game service should be started</param>
        public override void Prepare(Tuple<bool, string> parameter)
        {
            if (!_client.IsRunning)
                NavigationService.Navigate<HomeViewModel>();

            _playerName = parameter.Item2;

            if (parameter.Item1)
            {
                _isServer = true;
                _gameService.Start();
                _gameService.GameStopped += (_, __) => EOMT(UnsafeNavigateBack);
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

        private void UnsafeNavigateBack()
        {
            UnregisterEvents();

            if (_gameService.IsRunning)
                _gameService.Stop();

            _client.Stop();

            NavigationService.Navigate<HomeViewModel, string>(_playerName);
        }

        private void PickCards()
        {
            if (CzarMode)
            {
                NetDataWriter writer = NetHelpers.GetKeyValue(GameKey.CzarPick);
                // Require server confirmation before exiting czar mode?
            } else
            {
                NetDataWriter writer = NetHelpers.GetKeyValue(GameKey.ClientSendWhiteCards);
                foreach (WhiteCard card in SelectedWhiteCards)
                    writer.Put(card.Id);
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

        private void OnDisconnected(NetKey e)
        {
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

            UnsafeNavigateBack();
            _messenger.Send(new DialogMessage
            {
                Title = title,
                Content = message,
                Buttons = DialogMessage.Button.Ok
            });
        }

        private void UnregisterEvents()
        {
            if (_gameService.IsRunning)
                _gameService.GameStopped -= (_, __) => UnsafeNavigateBack();

            if (_client.IsRunning)
            {
                _client.GameEvent -= (_, e) => EOMT(() => OnGameEvent(e));
                _client.DisconnectedFromServer -= (_, e) => EOMT(() => OnDisconnected(e));
            }
        }
    }
}
