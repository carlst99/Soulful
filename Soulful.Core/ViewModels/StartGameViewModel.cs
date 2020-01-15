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

        private readonly INetServerService _server;
        private readonly INetClientService _client;
        private readonly IIntraMessenger _messenger;

        private int _gamePin;
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
            get => _gamePin.ToString("000000");
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
        public bool CanStartGame => _server.Players.Count >= MIN_PLAYERS - 1;

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
        public IMvxCommand StartGameCommand => new MvxCommand(StartGame);

        /// <summary>
        /// Kicks a connected player
        /// </summary>
        public IMvxCommand KickPlayerCommand => new MvxCommand<int>(KickPlayer);

        #endregion

        public StartGameViewModel(IMvxNavigationService navigationService,
            INetServerService server,
            INetClientService client,
            IIntraMessenger messenger)
            : base(navigationService)
        {
            _server = server;
            _client = client;
            _messenger = messenger;

            MaxPlayers = 20;
            Players = new ObservableCollection<Tuple<int, string>>();
            _server.PlayerConnected += (_, p) => OnPlayerCollectionChanged(p, true);
            _server.PlayerDisconnected += (_, p) => OnPlayerCollectionChanged(p, false);
        }

        public override Task Initialize()
        {
            GenerateGamePin();
            _server.Start(MaxPlayers, GamePin);
            _client.Start(GamePin, _playerName);
            return base.Initialize();
        }

        private async void StartGame()
        {
            if (!CanStartGame)
                return;

            UnregisterEvents();
            await NavigationService.Navigate<GameViewModel, bool>(true).ConfigureAwait(false);
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
                    Content = "You can't kick yourself doofus!",
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
                    Content = "People are already queueing up to play! Are you sure you want to deprive them of this wonderful opportunity by closing the server?",
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
            if (_server.IsRunning)
                _server.Stop();

            UnregisterEvents();
            NavigationService.Navigate<HomeViewModel>();
        }

        private void GenerateGamePin()
        {
            Random r = new Random();
            _gamePin = r.Next(100000, 999999);
            RaisePropertyChanged(nameof(GamePin));
            _server.ChangeConnectPin(GamePin);
        }

        private async void OnPlayerCollectionChanged(NetPeer peer, bool connected)
        {
            if (connected)
            {
                // Should capture this client every time, preventing the host from kicking themselves.
                // No one can join the game in 30ms (I hope lol)
                if (_players.Count == 0)
                    _clientId = peer.Id;
                await AsyncDispatcher.ExecuteOnMainThreadAsync(() => Players.Add(new Tuple<int, string>(peer.Id, (string)peer.Tag))).ConfigureAwait(false);
            } else
            {
                await AsyncDispatcher.ExecuteOnMainThreadAsync(() => Players.Remove(Players.First(p => p.Item1 == peer.Id))).ConfigureAwait(false);
            }
            await base.RaisePropertyChanged(nameof(CanStartGame)).ConfigureAwait(false);
        }

        private void UnregisterEvents()
        {
            _server.PlayerConnected -= (_, p) => OnPlayerCollectionChanged(p, true);
            _server.PlayerDisconnected -= (_, p) => OnPlayerCollectionChanged(p, false);
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
