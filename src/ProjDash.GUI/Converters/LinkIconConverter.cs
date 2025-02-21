// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;

using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;

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

        return values switch
        {
            [IconRef icon, string color] => Convert(icon, color),
            [IconRef icon, null] => Convert(icon),
            [IconRef icon] => Convert(icon),
            [null] or [null, _] => Convert(null),
            _ => throw new InvalidOperationException()
        };
    }

    public IImage? Convert(IconRef? icon, string? color = null)
    {
        var icons = App.Current?.MainWindow?.ViewModel?.Settings.IconImports ??
                    ImmutableDictionary<string, IconImport>.Empty;
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
            return new IconImage { Value = "mdi-link", Brush = colorBrush ?? Brushes.Black };
        }
    }

    private static IconImage? GetIconImage(IconRef.MaterialIcon materialIcon, IBrush? colorBrush)
    {
        try
        {
            return new IconImage { Value = materialIcon.Reference, Brush = colorBrush ?? Brushes.Black };
        }
        catch (Exception ex)
        {
            Log.Warning(ex, $"Could not load icon {materialIcon.Reference}");
            return null;
        }
    }

    private static Bitmap LoadBitmap(IconRef.DataUriIcon dataUriIcon)
    {
        var matches = DataUriRegex.Match(dataUriIcon.Reference);

        if (matches.Groups.Count < 3)
        {
            Log.Error("Invalid DataUrl format");
        }

        using var fileStream = new MemoryStream(System.Convert.FromBase64String(matches.Groups["data"].Value));
        return new Bitmap(fileStream);
    }

    [GeneratedRegex("data:(?<type>.+?);base64,(?<data>.+)")]
    private static partial Regex DataUriRegex { get; }

    public static LinkIconConverter Instance => new();
}