using MvvmCross;
using MvvmCross.Converters;
using Soulful.Core.Services;
using System;
using System.Globalization;

namespace Soulful.Core.Converters
{
    public class IntToCardConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int iValue)
                return Mvx.IoCProvider.Resolve<ICardLoaderService>().GetWhiteCardAsync(iValue).Result;
            else if (value is string sValue)
                return sValue;
            else
                throw new ArgumentException("Value is of incorrect type");
        }
    }
}
