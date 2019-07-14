using LiteNetLib.Utils;
using MvvmCross.Navigation;
using Soulful.Core.Net;
using System.Collections.ObjectModel;

namespace Soulful.Core.ViewModels
{
    public class GameViewModel : Base.ViewModelBase<string>
    {
        #region Fields

        private readonly INetClientService _client;
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

        public GameViewModel(IMvxNavigationService navigationService, INetClientService client)
            : base(navigationService)
        {
            _client = client;
            _client.GameEvent += OnGameEvent;

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
                        AsyncDispatcher.ExecuteOnMainThreadAsync(() => WhiteCards.Add(e.Data.GetInt()));
                    break;
            }
        }

        public override void Prepare(string parameter)
        {
            _playerName = parameter;
        }
    }
}
