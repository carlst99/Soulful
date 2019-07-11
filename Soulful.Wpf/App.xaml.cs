using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using MvvmCross;
using MvvmCross.Core;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.Platforms.Wpf.Views;
using Soulful.Core.ViewModels;

namespace Soulful.Wpf
{
    public partial class App : MvxApplication
    {
        private StartupEventArgs _startArgs;
        private Process _otherProcess;

        protected override void RegisterSetup()
        {
            this.RegisterSetupType<MvxWpfSetup<Core.App>>();
        }

#if DEBUG
        protected override void OnStartup(StartupEventArgs e)
        {
            _startArgs = e;
            base.OnStartup(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            if (_startArgs?.Args.Length > 0)
            {
                IMvxNavigationService navService = Mvx.IoCProvider.Resolve<IMvxNavigationService>();

                if (_startArgs.Args[0] == "autoServer")
                {
                    navService.Navigate<StartGameViewModel, string>("Server");
                    if (_startArgs.Args.Length > 1 && _startArgs.Args[1] == "launchOther")
                        _otherProcess = Process.Start("Soulful.Wpf.exe", "autoClient");
                }
                else if (_startArgs.Args[0] == "autoClient")
                {
                    navService.Navigate<JoinGameViewModel, string>("Client");
                    if (_startArgs.Args.Length > 1 && _startArgs.Args[1] == "launchOther")
                        _otherProcess = Process.Start("Soulful.Wpf.exe", "autoServer");
                }
                _startArgs = null;
            }
            base.OnActivated(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (!_otherProcess.HasExited)
                _otherProcess.CloseMainWindow();
            base.OnExit(e);
        }
#endif
    }
}
