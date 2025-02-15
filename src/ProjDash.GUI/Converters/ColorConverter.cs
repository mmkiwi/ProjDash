using System.Globalization;

using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace MMKiwi.ProjDash.GUI.Converters;

public class ColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return null;
        if (value is string strValue && targetType.IsAssignableTo(typeof(Color)))
        {
            return Color.TryParse(strValue, out Color result) ? result : null;
        }

        if (value is Color colorValue && targetType.IsAssignableTo(typeof(string)))
        {
            return colorValue.ToString();
        }

        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
    
    public static ColorConverter Instance => new ColorConverter();
}