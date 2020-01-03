using MvvmCross.Converters;
using System;
using System.Globalization;
using System.Windows;

namespace Soulful.Wpf.UI
{
    public class VisibilityValueConverter : MvxValueConverter<bool, Visibility>
    {
        protected override Visibility Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ? Visibility.Visible : Visibility.Hidden;
        }

        protected override bool ConvertBack(Visibility value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == Visibility.Visible;
        }
    }

    public class InvertedVisibilityValueConverter : MvxValueConverter<bool, Visibility>
    {
        protected override Visibility Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ? Visibility.Hidden : Visibility.Visible;
        }

        protected override bool ConvertBack(Visibility value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != Visibility.Visible;
        }
    }
}
