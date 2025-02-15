using System.Collections.Frozen;
using System.Collections.Immutable;
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
            case [IconRef icon, string color]:
                return Convert(icon, color);
            case [IconRef icon, null]:
                return Convert(icon);
            case [IconRef icon]:
                return Convert(icon);
            case [null]:
            case [null, _]:
                return Convert(null);
            default:
                throw new NotSupportedException();
        }
    }

    public IImage? Convert(IconRef? icon, string? color = null)
    {
        var icons = App.Current?.MainWindow?.ViewModel?.Settings.IconImports ?? ImmutableDictionary<string,IconImport>.Empty;
        IBrush? colorBrush = color is not null ? Brush.Parse(color) : null;
        return icon switch
        {
            IconRef.ImportIcon importIcon => icons.TryGetValue(importIcon.Reference, out IconImport? iconImport)
                ? iconImport.ToDrawingImage(colorBrush)
                : DefaultIcon(),
            IconRef.MaterialIcon materialIcon => GetIconImage(materialIcon, colorBrush),
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

    private IImage? GetIconImage(IconRef.MaterialIcon materialIcon, IBrush? colorBrush)
    {
        try
        {
            return new IconImage() { Value = materialIcon.Reference, Brush = colorBrush ?? Brushes.Black };
        }
        catch(Exception ex)
        {
            Log.Warning(ex, $"Could not load icon {materialIcon.Reference}");
            return null;
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