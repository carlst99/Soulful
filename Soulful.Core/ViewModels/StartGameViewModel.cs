using LiteNetLib;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
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

        public const double MIN_PLAYERS = 3;
        public const double MAX_PLAYERS = 100;

        #endregion

        #region Fields

        private readonly INetServerService _server;
        private int _gamePin;
        private int _maxPlayers;
        private string _playerName;
        private ObservableCollection<Tuple<int, string>> _players;

        #endregion

        #region Properties

        public string GamePin
        {
            get => _gamePin.ToString("000000");
        }

        public int MaxPlayers
        {
            get => _maxPlayers;
            set
            {
                SetProperty(ref _maxPlayers, value);
                _server.ChangeMaxPlayers(value);
            }
        }

        public ObservableCollection<Tuple<int, string>> Players
        {
            get => _players;
            set => SetProperty(ref _players, value);
        }

#if DEBUG
        public bool CanStartGame => _server.Players.Count >= 1;
#else
        public bool CanStartGame => Server.Players.Count >= MIN_PLAYERS - 1;
#endif

        #endregion

        #region Commands

        public IMvxCommand RefreshGamePinCommand => new MvxCommand(GenerateGamePin);
        public IMvxCommand NavigateBackCommand => new MvxCommand(NavigateBack);
        public IMvxCommand StartGameCommand => new MvxCommand(StartGame);
        public IMvxCommand KickPlayerCommand => new MvxCommand<int>((i) => _server.Kick(i));

        #endregion

        public StartGameViewModel(IMvxNavigationService navigationService, INetServerService server)
            : base(navigationService)
        {
            _server = server;
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

            IGameClientService client = Mvx.IoCProvider.Resolve<IGameClientService>();
            client.Start(GamePin, _playerName);
            await Task.Run(async () =>
            {
                while (!client.IsRunning)
                    await Task.Delay(15).ConfigureAwait(false);
            }).ConfigureAwait(false);

            await NavigationService.Navigate<GameViewModel, string>(_playerName).ConfigureAwait(false);
        }

        private void NavigateBack()
        {
            if (_server.IsRunning)
                _server.Stop();
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
            await RaisePropertyChanged(nameof(CanStartGame));
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
