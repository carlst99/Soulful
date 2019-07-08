using Soulful.Core.Resources;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Serilog;

namespace Soulful.Core.ViewModels.Base
{
    public abstract class ViewModelBase<T> : MvxViewModel<T>, IViewModelBase
    {
        public string this[string index] => AppStrings.ResourceManager.GetString(index);
        public IMvxNavigationService NavigationService { get; }

        protected ViewModelBase(IMvxNavigationService navigationService)
        {
            NavigationService = navigationService;
            Log.Verbose("Navigated to " + GetType().Name);
        }
    }

    public abstract class ViewModelBase : MvxViewModel, IViewModelBase
    {
        public string this[string index] => AppStrings.ResourceManager.GetString(index);
        public IMvxNavigationService NavigationService { get; }

        protected ViewModelBase(IMvxNavigationService navigationService)
        {
            NavigationService = navigationService;
            Log.Verbose("Navigated to " + GetType().Name);
        }
    }
}
