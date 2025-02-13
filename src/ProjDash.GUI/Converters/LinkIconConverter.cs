using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using DynamicData;

using MMKiwi.ProjDash.GUI.Helpers;
using MMKiwi.ProjDash.ViewModel.Model;

using Projektanker.Icons.Avalonia;

using Serilog;

namespace MMKiwi.ProjDash.GUI.Converters;

public partial class LinkIconConverter : IMultiValueConverter
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
            IconRef.DataUriIcon dataUriIcon => LoadBitmap(dataUriIcon),
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

    private Bitmap LoadBitmap(IconRef.DataUriIcon dataUriIcon)
    {
        var matches = DataUriRegex.Match(dataUriIcon.Reference);

        if (matches.Groups.Count < 3)
        {
            Log.Error("Invalid DataUrl format");
        }

        using var fileStream = new MemoryStream(System.Convert.FromBase64String(matches.Groups["data"].Value));
        return new Bitmap(fileStream);
    }

    [GeneratedRegex(@"data:(?<type>.+?);base64,(?<data>.+)")]
    public partial Regex DataUriRegex { get; }
    
    public static LinkIconConverter Instance => new();
}