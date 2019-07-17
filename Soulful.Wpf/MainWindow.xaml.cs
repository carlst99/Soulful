using AmRoMessageDialog;
using IntraMessaging;
using MvvmCross;
using MvvmCross.Platforms.Wpf.Views;
using Soulful.Core.Model;
using System;

namespace Soulful.Wpf
{
    public partial class MainWindow : MvxWindow
    {
        private readonly AmRoMessageBox _messageBox;

        public MainWindow()
        {
            InitializeComponent();

            _messageBox = new AmRoMessageBox
            {
                Background = "#333",
                TextColor = "#fff",
                RippleEffectColor = "#000",
                ClickEffectColor = "#1F2023",
                ShowMessageWithEffect = true,
                EffectArea = this,
                ParentWindow = this,
                IconColor = "#3399ff",
                CaptionFontSize = 16,
                MessageFontSize = 14
            };

            IIntraMessenger messenger = Mvx.IoCProvider.Resolve<IIntraMessenger>();
            messenger.Subscribe(OnMessage, new Type[] { typeof(DialogMessage) });
        }

        private void OnMessage(IMessage message)
        {
            if (message is DialogMessage dMessage)
            {
                AmRoMessageBoxButton buttons = AmRoMessageBoxButton.Ok;
                if ((dMessage.Buttons & DialogMessage.Button.Ok) != 0)
                {
                    if ((dMessage.Buttons & DialogMessage.Button.Cancel) != 0)
                        buttons = AmRoMessageBoxButton.OkCancel;
                    else
                        buttons = AmRoMessageBoxButton.Ok;
                } else if ((dMessage.Buttons & DialogMessage.Button.Yes) != 0)
                {
                    if ((dMessage.Buttons & DialogMessage.Button.Cancel) != 0)
                        buttons = AmRoMessageBoxButton.YesNoCancel;
                    else
                        buttons = AmRoMessageBoxButton.YesNo;
                }

                switch (_messageBox.Show(dMessage.Content, dMessage.Title, buttons))
                {
                    case AmRoMessageBoxResult.Cancel:
                        dMessage.Callback?.Invoke(DialogMessage.Button.Cancel);
                        break;
                    case AmRoMessageBoxResult.No:
                        dMessage.Callback?.Invoke(DialogMessage.Button.No);
                        break;
                    case AmRoMessageBoxResult.Ok:
                        dMessage.Callback?.Invoke(DialogMessage.Button.Ok);
                        break;
                    case AmRoMessageBoxResult.Yes:
                        dMessage.Callback?.Invoke(DialogMessage.Button.Yes);
                        break;
                }
            }
        }
    }
}
