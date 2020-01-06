using MvvmCross.Platforms.Wpf.Views;
using System.Text.RegularExpressions;

namespace Soulful.Wpf.Views
{
    public partial class JoinGameView : MvxWpfView
    {
        public JoinGameView()
        {
            InitializeComponent();
            TxtBxPin.Focus();
        }

        private void TxtBxPin_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Regex match = new Regex("[0-9]");
            if (!match.IsMatch(e.Key.ToString()))
                e.Handled = true;
        }
    }
}
