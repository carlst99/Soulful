using MvvmCross.Commands;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Soulful.Wpf.UI
{
    /// <summary>
    /// Interaction logic for WhiteCardControl.xaml
    /// </summary>
    public partial class WhiteCardControl : UserControl
    {
        #region Dependency Properties

        private static readonly DependencyPropertyKey SelectedCardsProperty =
            DependencyProperty.RegisterReadOnly(
                "SelectedCards",
                typeof(ObservableCollection<int>),
                typeof(WhiteCardControl),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = new ObservableCollection<int>()
                });

        public static readonly DependencyProperty CardsProperty =
            DependencyProperty.Register(
                "Cards",
                typeof(ObservableCollection<int>),
                typeof(WhiteCardControl),
                new FrameworkPropertyMetadata
                {
                    AffectsMeasure = true,
                    AffectsRender = true
                });

        public static readonly DependencyProperty SelectionEnabledProperty =
            DependencyProperty.Register(
                "SelectionEnabled",
                typeof(bool),
                typeof(WhiteCardControl),
                new FrameworkPropertyMetadata
                {
                    AffectsRender = true,
                    DefaultValue = false
                });

        public static readonly DependencyProperty MaxSelectionProperty = DependencyProperty.Register("MaxSelection", typeof(int), typeof(WhiteCardControl));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the list of white cards to display
        /// </summary>
        public ObservableCollection<int> Cards
        {
            get => (ObservableCollection<int>)GetValue(CardsProperty);
            set => SetValue(CardsProperty, value);
        }

        /// <summary>
        /// Gets the list of selected cards
        /// </summary>
        public ObservableCollection<int> SelectedCards
        {
            get => (ObservableCollection<int>)GetValue(SelectedCardsProperty.DependencyProperty);
        }

        /// <summary>
        /// Gets or sets a value indicating whether white cards can be selected or not. Should be used in tandem with <see cref="MaxSelection"/>
        /// </summary>
        public bool SelectionEnabled
        {
            get => (bool)GetValue(SelectionEnabledProperty);
            set => SetValue(SelectionEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets the maximum number of cards that can be selected
        /// </summary>
        public int MaxSelection
        {
            get => (int)GetValue(MaxSelectionProperty);
            set => SetValue(MaxSelectionProperty, value);
        }

        #endregion

        #region Commands

        public IMvxCommand<int> OnCheckedCommand => new MvxCommand<int>(OnChecked);
        public IMvxCommand<int> OnUncheckedCommand => new MvxCommand<int>(OnUnchecked);

        #endregion

        public WhiteCardControl()
        {
            InitializeComponent();
        }

        private void OnChecked(int card)
        {
            if (SelectionEnabled && Cards.Contains(card))
                SelectedCards.Add(card);

            if (SelectedCards.Count == MaxSelection)
                SelectionEnabled = false;
        }

        private void OnUnchecked(int card)
        {
            if (SelectedCards.Contains(card))
                SelectedCards.Remove(card);

            if (!SelectionEnabled)
                SelectionEnabled = true;
        }
    }
}
