using MvvmCross.Commands;
using MvvmCross.Navigation;

namespace Soulful.Core.ViewModels
{
    public class HomeViewModel : Base.ViewModelBase
    {
        #region Fields

        private string _playerName;

        #endregion

        #region Properties

        public string PlayerName
        {
            get => _playerName;
            set
            {
                SetProperty(ref _playerName, value);
                RaisePropertyChanged(nameof(CanNavigateToGamePlay));
            }
        }

        public bool CanNavigateToGamePlay => !string.IsNullOrEmpty(PlayerName);

        #endregion

        #region Commands

        public IMvxCommand StartGameCommand => new MvxCommand(() => NavigationService.Navigate<StartGameViewModel, string>(PlayerName));

        public IMvxCommand JoinGameCommand => new MvxCommand(() => NavigationService.Navigate<JoinGameViewModel, string>(PlayerName));

        public IMvxCommand BrowseCardsCommand => new MvxCommand(() => NavigationService.Navigate<CardBrowserViewModel>());

        #endregion

        public HomeViewModel(IMvxNavigationService navigationService)
            : base(navigationService)
        {
        }
    }
}
