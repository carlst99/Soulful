using MvvmCross.Commands;
using MvvmCross.Navigation;
using Soulful.Core.Model.Cards;
using Soulful.Core.Services;
using System.Collections.Generic;

namespace Soulful.Core.ViewModels
{
    public class CardBrowserViewModel : Base.ViewModelBase
    {
        #region Fields

        private List<Pack> _cardPacks;
        private Pack _selectedPack;

        #endregion

        #region Properties

        public List<Pack> CardPacks
        {
            get => _cardPacks;
            set => SetProperty(ref _cardPacks, value);
        }

        public Pack SelectedPack
        {
            get => _selectedPack;
            set => SetProperty(ref _selectedPack, value);
        }

        #endregion

        public IMvxCommand NavigateBackCommand => new MvxCommand(() => NavigationService.Navigate<HomeViewModel>());

        public CardBrowserViewModel(IMvxNavigationService navigationService, ICardLoaderService cardLoader)
            : base(navigationService)
        {
            CardPacks = cardLoader.Packs;
            if (CardPacks.Count > 0)
                SelectedPack = CardPacks[0];
        }
    }
}
