using MvvmCross.Commands;
using MvvmCross.Navigation;
using Realms;
using Soulful.Core.Model.CardDb;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Soulful.Core.ViewModels
{
    public class CardBrowserViewModel : Base.ViewModelBase
    {
        #region Fields

        private readonly Realm _cardsRealm;
        private IQueryable<Pack> _cardPacks;
        private Pack _selectedPack;

        #endregion

        #region Properties

        public IQueryable<Pack> CardPacks
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

        public CardBrowserViewModel(IMvxNavigationService navigationService)
            : base(navigationService)
        {
            _cardsRealm = RealmHelpers.GetCardsRealm();
            CardPacks = _cardsRealm.All<Pack>();
            if (CardPacks.Count() > 0)
                SelectedPack = CardPacks.ElementAt(0);
        }
    }
}
