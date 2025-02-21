// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

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