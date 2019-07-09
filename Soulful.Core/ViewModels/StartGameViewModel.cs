using MvvmCross.Navigation;

namespace Soulful.Core.ViewModels
{
    public class StartGameViewModel : Base.ViewModelBase
    {
        private int _gamePin;

        public int GamePin
        {
            get => _gamePin;
            set => SetProperty(ref _gamePin, value);
        }

        public StartGameViewModel(IMvxNavigationService navigationService)
            : base(navigationService)
        {
        }
    }
}
