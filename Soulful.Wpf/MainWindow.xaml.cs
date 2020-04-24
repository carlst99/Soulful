using IntraMessaging;
using MaterialDesignThemes.Wpf;
using MvvmCross;
using MvvmCross.Platforms.Wpf.Views;
using Soulful.Core.Model;
using Soulful.Wpf.Views;
using System;
using System.Threading.Tasks;

namespace Soulful.Wpf
{
    public partial class MainWindow : MvxWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            IIntraMessenger messenger = Mvx.IoCProvider.Resolve<IIntraMessenger>();
            messenger.Subscribe(async (m) => await OnMessage(m).ConfigureAwait(false), new Type[] { typeof(DialogMessage) });
        }

        private async Task OnMessage(IMessage message)
        {
            if (message is DialogMessage d)
            {
                bool value = (bool)await Dispatcher.Invoke(async () =>
                {
                    MessageDialog dialog = new MessageDialog(d.Message, d.Title, d.OkayButtonContent, d.CancelButtonContent, d.HelpUrl);
                    return await DialogHost.Show(dialog).ConfigureAwait(false);
                }).ConfigureAwait(false);
                d.Callback?.Invoke(value);
            }
        }
    }
}
