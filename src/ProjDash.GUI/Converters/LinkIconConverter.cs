using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;

using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;

using DynamicData;

using MMKiwi.ProjDash.GUI.Helpers;
using MMKiwi.ProjDash.ViewModel.Model;

using Projektanker.Icons.Avalonia;

namespace MMKiwi.ProjDash.GUI.Converters;

public class LinkIconConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!targetType.IsAssignableTo(typeof(IImage)))
        {
            throw new NotSupportedException();
        }

        // Still loading
        if (values.Any(v => v is UnsetValueType))
            return null;

        switch (values)
        {
            case [IconRef icon, IReadOnlyDictionary<string, IconImport> icons, string color]:
                return Convert(icon, icons, color);
            case [IconRef icon, IReadOnlyDictionary<string, IconImport> icons, null]:
                return Convert(icon, icons);
            case [IconRef icon, IReadOnlyDictionary<string, IconImport> icons]:
                return Convert(icon, icons);
            case [null, _, _]:
            case [null, _]:
                return Convert(null, FrozenDictionary<string, IconImport>.Empty, null);
            default:
                throw new NotSupportedException();
        }
    }

    public IImage? Convert(IconRef? icon, IReadOnlyDictionary<string, IconImport> icons, string? color = null)
    {
        IBrush? colorBrush = color is not null ? Brush.Parse(color) : null;
        return icon switch
        {
            IconRef.ImportIcon importIcon => icons.TryGetValue(importIcon.Reference, out IconImport? iconImport)
                ? iconImport.ToDrawingImage(colorBrush)
                : DefaultIcon(),
            IconRef.MaterialIcon materialIcon => new IconImage()
            {
                Value = materialIcon.Reference, Brush = colorBrush ?? Brushes.Black
            },
            _ => DefaultIcon()
        };

        IconImage DefaultIcon()
        {
            return new IconImage()
            {
                Value = "mdi-link", Brush = colorBrush ?? Brushes.Black
            };
        }
    }

    public static LinkIconConverter Instance => new();
}