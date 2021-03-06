﻿using MvvmCross.Converters;
using System;
using System.Globalization;

namespace Soulful.Core.Converters
{
    public class BoolInverterConverter : MvxValueConverter<bool, bool>
    {
        protected override bool Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value;
        }

        protected override bool ConvertBack(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value;
        }
    }
}
