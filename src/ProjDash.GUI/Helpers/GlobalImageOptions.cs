using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace MMKiwi.ProjDash.GUI.Helpers;

public static class GlobalImageOptions
{
    public static readonly AttachedProperty<BitmapInterpolationMode> InterpolationModeProperty =
        AvaloniaProperty.RegisterAttached<Image, BitmapInterpolationMode>(
            "InterpolationMode",
            typeof(GlobalImageOptions),
            defaultValue: BitmapInterpolationMode.HighQuality);

    public static void SetInterpolationMode(Image image, BitmapInterpolationMode value)
    {
        RenderOptions.SetBitmapInterpolationMode(image, value);
    }
}