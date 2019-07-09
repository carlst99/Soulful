using MvvmCross.Commands;
using MvvmCross.Navigation;
using System.Collections.ObjectModel;

namespace Soulful.Core.ViewModels
{
    public class CardBrowserViewModel : Base.ViewModelBase
    {
        private ObservableCollection<string> _whiteCards;
        private ObservableCollection<string> _blackCards;

        public ObservableCollection<string> WhiteCards
        {
            get => _whiteCards;
            set => SetProperty(ref _whiteCards, value);
        }

        public ObservableCollection<string> BlackCards
        {
            get => _blackCards;
            set => SetProperty(ref _blackCards, value);
        }

        public IMvxCommand NavigateBackCommand => new MvxCommand(() => NavigationService.Navigate<HomeViewModel>());

        public CardBrowserViewModel(IMvxNavigationService navigationService)
            : base(navigationService)
        {
            WhiteCards = new ObservableCollection<string>();
            BlackCards = new ObservableCollection<string>();

            for (int i = 0; i < 10; i++)
                WhiteCards.Add("test" + i.ToString());
        }
    }
}
