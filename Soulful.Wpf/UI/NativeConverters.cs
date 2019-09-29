using MvvmCross.Platforms.Wpf.Converters;
using MvvmCross.Plugin.Visibility;
using Soulful.Core.Converters;

namespace Soulful.Wpf.UI
{
    public class NativeIntToCardConverter : MvxNativeValueConverter<IntToCardConverter> { }

    public class NativeBoolInverterConverter : MvxNativeValueConverter<BoolInverterConverter> { }

    public class NativeMvxVisibilityValueConverter : MvxNativeValueConverter<MvxVisibilityValueConverter> { }

    public class NativeMvxInvertedVisibilityValueConverter : MvxNativeValueConverter<MvxInvertedVisibilityValueConverter> { }
}
