﻿using MvvmCross.Commands;
using System;
using System.Collections.ObjectModel;
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

        public static readonly DependencyProperty SelectedCardsProperty =
            DependencyProperty.Register(
                "SelectedCards",
                typeof(ObservableCollection<int>),
                typeof(WhiteCardControl),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = new ObservableCollection<int>()
                });

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
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
        public ObservableCollection<int> ItemsSource
        {
            get => (ObservableCollection<int>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Gets the list of selected cards
        /// </summary>
        public ObservableCollection<int> SelectedCards
        {
            get => (ObservableCollection<int>)GetValue(SelectedCardsProperty);
            set => throw new InvalidOperationException("Cannot modify a read-only dependency property");
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
            if (SelectionEnabled && ItemsSource.Contains(card))
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