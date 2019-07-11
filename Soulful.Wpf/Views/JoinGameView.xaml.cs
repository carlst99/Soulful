using MvvmCross.Platforms.Wpf.Views;

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
