﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Soulful.Wpf.UIEnhancements
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible ? true : false;
        }
    }
}