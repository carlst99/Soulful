using IntraMessaging;
using LiteNetLib;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Model;
using Soulful.Core.Net;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Soulful.Core.ViewModels
{
    public class StartGameViewModel : Base.ViewModelBase<string>
    {
        #region Constants

#if DEBUG
        public const double MIN_PLAYERS = 2;
#else
        public const double MIN_PLAYERS = 3;
#endif
        public const double MAX_PLAYERS = 16;

        #endregion

        #region Fields

        private readonly NetServerService _server;
        private readonly NetClientService _client;
        private readonly IIntraMessenger _messenger;

        private string _gamePin;
        private int _maxPlayers;
        private string _playerName;
        private ObservableCollection<Tuple<int, string>> _players;

        /// <summary>
        /// Stores the ID of the client, used to ensure that the host cannot kick themselves
        /// </summary>
        private int _clientId;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the pin used by peers to connect to the server
        /// </summary>
        public string GamePin
        {
            get => _gamePin;
            set => SetProperty(ref _gamePin, value);
        }

        /// <summary>
        /// Gets or sets the maximum number of players allowed in the server
        /// </summary>
        public int MaxPlayers
        {
            get => _maxPlayers;
            set
            {
                SetProperty(ref _maxPlayers, value);
                _server.ChangeMaxPlayers(value);
            }
        }

        /// <summary>
        /// Gets or sets the list of connected players
        /// </summary>
        public ObservableCollection<Tuple<int, string>> Players
        {
            get => _players;
            set => SetProperty(ref _players, value);
        }

        /// <summary>
        /// Gets a value indicating whether the game can be started
        /// </summary>
        public bool CanStartGame => _server.Players.Count >= MIN_PLAYERS;

        #endregion

        #region Commands

        /// <summary>
        /// Generates a new game pin
        /// </summary>
        public IMvxCommand RefreshGamePinCommand => new MvxCommand(GenerateGamePin);

        /// <summary>
        /// Asks the user if they really wish to navigate back, and performs the action if so
        /// </summary>
        public IMvxCommand NavigateBackCommand => new MvxCommand(NavigateBack);

        /// <summary>
        /// Starts the game
        /// </summary>
        public IMvxCommand StartGameCommand => new MvxAsyncCommand(StartGame);

        /// <summary>
        /// Kicks a connected player
        /// </summary>
        public IMvxCommand KickPlayerCommand => new MvxCommand<int>(KickPlayer);

        #endregion

        public StartGameViewModel(IMvxNavigationService navigationService,
            NetServerService server,
            NetClientService client,
            IIntraMessenger messenger)
            : base(navigationService)
        {
            _server = server;
            _client = client;
            _messenger = messenger;

            MaxPlayers = (int)MAX_PLAYERS;
            Players = new ObservableCollection<Tuple<int, string>>();
            _server.PlayerConnected += OnPlayerConnected;
            _server.PlayerDisconnected += OnPlayerDisconnected;
        }

        public async override Task Initialize()
        {
            GenerateGamePin();
            _server.Start(MaxPlayers, GamePin);
            await _client.Start(GamePin, _playerName).ConfigureAwait(false);
            _clientId = _server.Players[0].Id;
            await base.Initialize().ConfigureAwait(false);
            return;
        }

        private async Task StartGame()
        {
            if (!CanStartGame)
                return;

            UnregisterEvents();
            await NavigationService.Navigate<GameViewModel, Tuple<bool, string>>(new Tuple<bool, string>(true, _playerName)).ConfigureAwait(false);
        }

        private void KickPlayer(int playerId)
        {
            if (playerId != _clientId)
            {
                _server.Kick(playerId);
            }
            else
            {
                _messenger.Send(new DialogMessage
                {
                    Message = "You can't kick yourself doofus!",
                    Buttons = DialogMessage.Button.Ok,
                    Title = "Insecurity lvl 100"
                });
            }
        }

        private void NavigateBack()
        {
            if (_server.Players.Count > 1)
            {
                void callback(DialogMessage.Button button)
                {
                    if (button == DialogMessage.Button.Yes)
                        UnsafeNavigateBack();
                }

                _messenger.Send(new DialogMessage
                {
                    Title = "Oh, come on...",
                    Message = "People are already queueing up to play! Are you sure you want to deprive them of this wonderful opportunity by closing the server?",
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
            _client.Stop();
            if (_server.IsRunning)
                _server.Stop();

            UnregisterEvents();
            NavigationService.Navigate<HomeViewModel, string>(_playerName);
        }

        private void GenerateGamePin()
        {
            Random r = new Random();
            GamePin = r.Next(100000, 999999).ToString("000000");
            _server.ChangeConnectPin(GamePin);
        }

        private void OnPlayerConnected(object sender, NetPeer peer)
        {
            EOMT(() => Players.Add(new Tuple<int, string>(peer.Id, (string)peer.Tag)));
            RaisePropertyChanged(nameof(CanStartGame));
        }

        private void OnPlayerDisconnected(object sender, NetPeer peer)
        {
            Tuple<int, string> player = Players.First(p => p.Item1 == peer.Id);
            if (player != null)
                EOMT(() => Players.Remove(player));
            RaisePropertyChanged(nameof(CanStartGame));
        }

        private void UnregisterEvents()
        {
            _server.PlayerConnected -= OnPlayerConnected;
            _server.PlayerDisconnected -= OnPlayerDisconnected;
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
