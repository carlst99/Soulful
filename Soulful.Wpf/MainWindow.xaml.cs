using IntraMessaging;
using MvvmCross;
using MvvmCross.Platforms.Wpf.Views;
using Soulful.Core.Model;
using System;
using System.Windows;

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
                MessageBoxButton buttons = MessageBoxButton.OK;
                if ((dMessage.Buttons & DialogMessage.Button.Ok) != 0)
                {
                    if ((dMessage.Buttons & DialogMessage.Button.Cancel) != 0)
                        buttons = MessageBoxButton.OKCancel;
                    else
                        buttons = MessageBoxButton.OK;
                }
                else if ((dMessage.Buttons & DialogMessage.Button.Yes) != 0)
                {
                    if ((dMessage.Buttons & DialogMessage.Button.Cancel) != 0)
                        buttons = MessageBoxButton.YesNoCancel;
                    else
                        buttons = MessageBoxButton.YesNo;
                }

                switch (Dispatcher.Invoke(() => MessageBox.Show(dMessage.Content, dMessage.Title, buttons)))
                {
                    case MessageBoxResult.Cancel:
                        dMessage.Callback?.Invoke(DialogMessage.Button.Cancel);
                        break;
                    case MessageBoxResult.No:
                        dMessage.Callback?.Invoke(DialogMessage.Button.No);
                        break;
                    case MessageBoxResult.OK:
                        dMessage.Callback?.Invoke(DialogMessage.Button.Ok);
                        break;
                    case MessageBoxResult.Yes:
                        dMessage.Callback?.Invoke(DialogMessage.Button.Yes);
                        break;
                }
            }
        }
    }
}
