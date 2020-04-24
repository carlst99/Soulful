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

        public MessageDialog(string message, string title, string okayButton, string cancelButton = null, string helpUrl = null)
        {
            InitializeComponent();
            DataContext = this;

            Message = message;
            Title = title;
            OkayButtonContent = okayButton;
            CancelButtonContent = cancelButton;
            HelpUrl = string.IsNullOrEmpty(helpUrl) ? helpUrl : HelpUrls.Default;
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
