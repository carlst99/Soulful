using MvvmCross.Navigation;

namespace Soulful.Core.ViewModels
{
    public class HomeViewModel : Base.ViewModelBase
    {
        public HomeViewModel(IMvxNavigationService navigationService)
            : base(navigationService)
        {
        }
    }
}
