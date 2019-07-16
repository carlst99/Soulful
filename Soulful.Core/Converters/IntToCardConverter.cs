﻿using MvvmCross;
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
            {
                if (parameter == null)
                {
                    return Mvx.IoCProvider.Resolve<ICardLoaderService>().GetWhiteCardAsync(iValue).Result;
                }
                else if (parameter is string s)
                {
                    Tuple<string, int> card = Mvx.IoCProvider.Resolve<ICardLoaderService>().GetBlackCardAsync(iValue).Result;

                    if (string.Equals(s, "BlackText", StringComparison.OrdinalIgnoreCase))
                        return card.Item1;
                    else if (string.Equals(s, "BlackPick", StringComparison.OrdinalIgnoreCase))
                        return card.Item2;
                    else
                        throw new ArgumentException("Parameter string was invalid - use either 'BlackText' or 'BlackPick'");
                }
                else
                {
                    throw new ArgumentException("Parameter must be a string containing either 'BlackText' or 'BlackPick'");
                }
            }
            else if (value is string sValue)
            {
                return sValue;
            }
            else
            {
                throw new ArgumentException("Value is of incorrect must either be string or int");
            }
        }
    }
}
