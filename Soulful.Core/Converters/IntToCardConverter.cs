using MvvmCross;
using MvvmCross.Converters;
using Soulful.Core.Services;
using System;
using System.Globalization;

namespace Soulful.Core.Converters
{
    public class IntToCardConverter : MvxValueConverter<int, string>
    {
        protected override string Convert(int value, Type targetType, object parameter, CultureInfo culture)
        {
            return Mvx.IoCProvider.Resolve<ICardLoaderService>().GetWhiteCardAsync(value).Result;
        }
    }
}
