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
        #region Fields

        private readonly ICardLoaderService _cardLoader;

        private ObservableCollection<string> _whiteCards;
        private ObservableCollection<Tuple<string, int>> _blackCards;
        private List<PackInfo> _cardPacks;
        private PackInfo _selectedPack;

        #endregion

        #region Properties

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

        public List<PackInfo> CardPacks
        {
            get => _cardPacks;
            set => SetProperty(ref _cardPacks, value);
        }

        public PackInfo SelectedPack
        {
            get => _selectedPack;
            set
            {
                SetProperty(ref _selectedPack, value);
                LoadCards(value.Key);
            }
        }

        #endregion

        public IMvxCommand NavigateBackCommand => new MvxCommand(() => NavigationService.Navigate<HomeViewModel>());

        public CardBrowserViewModel(IMvxNavigationService navigationService, ICardLoaderService cardLoader)
            : base(navigationService)
        {
            _cardLoader = cardLoader;
            CardPacks = cardLoader.Packs;
            if (CardPacks.Count > 0)
                SelectedPack = CardPacks[0];
        }

        #region Card Loading

        private async void LoadCards(string packKey)
        {
            await Task.WhenAll(LoadBlackCards(packKey), LoadWhiteCards(packKey)).ConfigureAwait(false);
        }

        private async Task LoadBlackCards(string packKey)
        {
            BlackCards = new ObservableCollection<Tuple<string, int>>(await _cardLoader.GetPackBlackCardsAsync(packKey).ConfigureAwait(false));
        }

        private async Task LoadWhiteCards(string packKey)
        {
            WhiteCards = new ObservableCollection<string>(await _cardLoader.GetPackWhiteCardsAsync(packKey).ConfigureAwait(false));
        }

        #endregion
    }
}
