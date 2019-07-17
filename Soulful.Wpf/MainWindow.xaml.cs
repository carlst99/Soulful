using IntraMessaging;
using MvvmCross;
using MvvmCross.Platforms.Wpf.Views;
using Soulful.Core.Model;
using System;

namespace Soulful.Wpf
{
    public partial class MainWindow : MvxWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            IIntraMessenger messenger = Mvx.IoCProvider.Resolve<IIntraMessenger>();
            messenger.Subscribe(OnMessage, new Type[] { typeof(DialogMessage) });
        }

        private void OnMessage(IMessage message)
        {
            if (message is DialogMessage dMessage)
            {

            }
        }
    }
}
