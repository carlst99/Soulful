using Soulful.Core.Resources;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Serilog;
using System;

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

        /// <summary>
        /// Provides a syntatic shortcut to <see cref="AsyncDispatcher.ExecuteOnMainThreadAsync"/>
        /// </summary>
        /// <param name="action">The action to execute</param>
        protected void EOMT(Action action) => AsyncDispatcher.ExecuteOnMainThreadAsync(action);
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

        /// <summary>
        /// Provides a syntatic shortcut to <see cref="AsyncDispatcher.ExecuteOnMainThreadAsync"/>
        /// </summary>
        /// <param name="action">The action to execute</param>
        protected void EOMT(Action action) => AsyncDispatcher.ExecuteOnMainThreadAsync(action);
    }
}
