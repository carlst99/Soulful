using IntraMessaging;
using LiteNetLib;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Model;
using Soulful.Core.Net;
using Soulful.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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
        public const double MAX_PLAYERS = 100;

        #endregion

        #region Fields

        private readonly INetServerService _server;
        private readonly INetClientService _client;
        private readonly IIntraMessenger _messenger;

        private int _gamePin;
        private int _maxPlayers;
        private string _playerName;
        private ObservableCollection<Tuple<int, string>> _players;

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
        public IMvxCommand KickPlayerCommand => new MvxCommand<int>((i) => _server.Kick(i));

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

            GenerateGamePin();
            _server.Players.CollectionChanged += OnPlayerCollectionChanged;
            _server.Start(MaxPlayers, GamePin);
        }

        private async void StartGame()
        {
            if (!CanStartGame)
                return;

            _server.Players.CollectionChanged -= OnPlayerCollectionChanged;
            _client.ConnectLocal(GamePin, _playerName);
            await NavigationService.Navigate<GameViewModel, bool>(true).ConfigureAwait(false);
        }

        private void NavigateBack()
        {
            if (_server.Players.Count > 0)
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
            if (_server.IsRunning)
            {
                _server.Stop();
            }

            _server.Players.CollectionChanged -= OnPlayerCollectionChanged;
            NavigationService.Navigate<HomeViewModel>();
        }

        private void GenerateGamePin()
        {
            Random r = new Random();
            _gamePin = r.Next(100000, 999999);
            RaisePropertyChanged(nameof(GamePin));
            _server.ChangeConnectPin(GamePin);
        }

        private async void OnPlayerCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (NetPeer element in e.NewItems)
                {
                    await AsyncDispatcher.ExecuteOnMainThreadAsync(() => Players.Add(new Tuple<int, string>(element.Id, (string)element.Tag))).ConfigureAwait(false);
                }
            } else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (NetPeer element in e.OldItems)
                {
                    await AsyncDispatcher.ExecuteOnMainThreadAsync(() => Players.Remove(Players.First(p => p.Item1 == element.Id))).ConfigureAwait(false);
                }
            }
            await RaisePropertyChanged(nameof(CanStartGame)).ConfigureAwait(false);
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
