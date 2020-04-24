using Soulful.Core.Model;
using Soulful.Core.Resources;
using System.Diagnostics;
using System.Windows.Controls;

namespace Soulful.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog : UserControl
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string OkayButtonContent { get; set; }
        public string CancelButtonContent { get; set; }
        public string HelpUrl { get; set; }

        public MessageDialog(string message, string title, string okayButton, string cancelButton, string helpUrl)
        {
            InitializeComponent();
            DataContext = this;

            Message = message;
            Title = title;
            OkayButtonContent = okayButton;
            CancelButtonContent = cancelButton;
            HelpUrl = string.IsNullOrEmpty(helpUrl) ? helpUrl : HelpUrls.Default;
        }

        public MessageDialog(DialogMessage dMessage)
            : this(dMessage.Message, dMessage.Title, dMessage.OkayButtonContent, dMessage.CancelButtonContent, dMessage.HelpUrl)
        {

        }

        private void OpenHelpUrl(object sender, System.Windows.RoutedEventArgs e)
        {
            if (HelpUrl != null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = HelpUrl,
                    UseShellExecute = true
                });
            }
        }
    }
}
