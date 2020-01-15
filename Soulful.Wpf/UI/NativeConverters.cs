using MvvmCross.Platforms.Wpf.Converters;
using Soulful.Core.Converters;

namespace Soulful.Wpf.UI
{
    public class NativeIntToCardConverter : MvxNativeValueConverter<IntToCardConverter> { }

    public class NativeBoolInverterConverter : MvxNativeValueConverter<BoolInverterConverter> { }

    public class NativeVisibilityValueConverter : MvxNativeValueConverter<VisibilityValueConverter> { }

    public class NativeInvertedVisibilityValueConverter : MvxNativeValueConverter<InvertedVisibilityValueConverter> { }
}
