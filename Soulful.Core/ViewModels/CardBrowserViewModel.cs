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
        private string _selectedPack;

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

        public string SelectedPack
        {
            get => _selectedPack;
            set
            {
                SetProperty(ref _selectedPack, value);
                LoadCards();
            }
        }

        public IMvxCommand NavigateBackCommand => new MvxCommand(() => NavigationService.Navigate<HomeViewModel>());

        public CardBrowserViewModel(IMvxNavigationService navigationService, ICardLoaderService cardLoader)
            : base(navigationService)
        {
            WhiteCards = new ObservableCollection<string>();
            BlackCards = new ObservableCollection<Tuple<string, int>>();
            CardPacks = cardLoader.Packs;
            SelectedPack = CardPacks?.Keys.First();
            _cardLoader = cardLoader;
        }

        private async Task LoadCards()
        {
            await Task.WhenAll(LoadBlackCards(), LoadWhiteCards()).ConfigureAwait(false);
        }

        private async Task LoadBlackCards()
        {
            foreach (Tuple<string, int> element in await _cardLoader.GetBlackCardsAsync(SelectedPack).ConfigureAwait(false))
                BlackCards.Add(element);
        }

        private async Task LoadWhiteCards()
        {
            foreach (string element in await _cardLoader.GetWhiteCardsAsync(SelectedPack).ConfigureAwait(false))
                WhiteCards.Add(element);
        }
    }
}
