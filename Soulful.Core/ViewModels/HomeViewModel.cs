using MvvmCross.Commands;
using MvvmCross.Navigation;
using System;

namespace Soulful.Core.ViewModels
{
    public class HomeViewModel : Base.ViewModelBase
    {
        public IMvxCommand StartGameCommand => new MvxCommand(StartGame);
        public IMvxCommand JoinGameCommand => new MvxCommand(JoinGame);
        public IMvxCommand BrowseCardsCommand => new MvxCommand(() => NavigationService.Navigate<CardBrowserViewModel>());

        public HomeViewModel(IMvxNavigationService navigationService)
            : base(navigationService)
        {
        }

        private void StartGame()
        {
            throw new NotImplementedException();
        }

        private void JoinGame()
        {
            throw new NotImplementedException();
        }
    }
}
