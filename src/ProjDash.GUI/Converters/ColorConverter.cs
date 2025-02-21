// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Globalization;

using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace MMKiwi.ProjDash.GUI.Converters;

public class ColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            null => null,
            string strValue when targetType.IsAssignableTo(typeof(Color)) => Color.TryParse(strValue, out Color result)
                ? result
                : null,
            Color colorValue when targetType.IsAssignableTo(typeof(string)) => colorValue.ToString(),
            _ => new BindingNotification(new InvalidCastException(), BindingErrorType.Error)
        };

        // converter used for the wrong type
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }

    public static ColorConverter Instance => new();
}