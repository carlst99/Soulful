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
    }
}
