using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Soulful.Core.ViewModels
{
    public class CardBrowserViewModel : Base.ViewModelBase
    {
        private readonly ICardLoaderService _cardLoader;

        private ObservableCollection<string> _whiteCards;
        private ObservableCollection<Tuple<string, int>> _blackCards;
        private Dictionary<string, PackInfo> _cardPacks;

        public ObservableCollection<string> WhiteCards
        {
            get => _whiteCards;
            set => SetProperty(ref _whiteCards, value);
        }

        public ObservableCollection<Tuple<string, int>> BlackCards
        {
            get => _blackCards;
            set => SetProperty(ref _blackCards, value);
        }

        public Dictionary<string, PackInfo> CardPacks
        {
            get => _cardPacks;
            set => SetProperty(ref _cardPacks, value);
        }

        public IMvxCommand NavigateBackCommand => new MvxCommand(() => NavigationService.Navigate<HomeViewModel>());
        public IMvxCommand ChangeSelectedPackCommand => new MvxCommand<string>(LoadCards);

        public CardBrowserViewModel(IMvxNavigationService navigationService, ICardLoaderService cardLoader)
            : base(navigationService)
        {
            _cardLoader = cardLoader;
            CardPacks = cardLoader.Packs;
            LoadCards(CardPacks.Keys.First());
        }

        #region Card Loading

        private async void LoadCards(string selectedPack)
        {
            await Task.WhenAll(LoadBlackCards(selectedPack), LoadWhiteCards(selectedPack)).ConfigureAwait(false);
        }

        private async Task LoadBlackCards(string selectedPack)
        {
            BlackCards = new ObservableCollection<Tuple<string, int>>(await _cardLoader.GetPackBlackCardsAsync(selectedPack).ConfigureAwait(false));
        }

        private async Task LoadWhiteCards(string selectedPack)
        {
            WhiteCards = new ObservableCollection<string>(await _cardLoader.GetPackWhiteCardsAsync(selectedPack).ConfigureAwait(false));
        }

        #endregion
    }
}
