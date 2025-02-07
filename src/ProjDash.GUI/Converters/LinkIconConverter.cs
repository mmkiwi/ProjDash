using System.Globalization;
using System.Reflection;

using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;

using MMKiwi.ProjDash.ViewModel.Model;

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
            case [UriType uriType, string color]:
                return Convert(uriType, color);
            case [UriType uriType, null]:
                return Convert(uriType);
            case [UriType uriType]:
                return Convert(uriType);
            default:
                throw new NotSupportedException();
        }
    }

    public IImage? Convert(UriType uriType, string? color = null)
    {
        IBrush? colorBrush = color is not null ? Brush.Parse(color) : null;
        return uriType switch
        {
            UriType.VantagePoint =>
                UpdateBrush(GetXaml("MMKiwi.ProjDash.GUI.Icons.VantagePoint.xaml"),
                    colorBrush ?? new SolidColorBrush(0xff03406c)),
            UriType.ProjectWise =>
                UpdateBrush(GetXaml("MMKiwi.ProjDash.GUI.Icons.ProjectWise.xaml"),
                    colorBrush ?? new SolidColorBrush(0xff60bb46)),
            UriType.GFS => new Projektanker.Icons.Avalonia.IconImage()
            {
                Value = "mdi-folder-network", Brush = colorBrush ?? Brushes.Black,
            },
            UriType.File => new Projektanker.Icons.Avalonia.IconImage()
            {
                Value = "mdi-folder", Brush = colorBrush ?? Brushes.Black,
            },
            UriType.Website => new Projektanker.Icons.Avalonia.IconImage()
            {
                Value = "mdi-folder-network", Brush = colorBrush ?? Brushes.Black,
            },
            _ => new Projektanker.Icons.Avalonia.IconImage() { Value = "mdi-link", Brush = colorBrush ?? Brushes.Black }
        };
    }

    private DrawingGroup? GetXaml(string xamlName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var names = assembly.GetManifestResourceNames();
        using Stream stream = assembly.GetManifestResourceStream(xamlName)!;
        return AvaloniaRuntimeXamlLoader.Load(stream) as DrawingGroup;
    }

    private IImage? UpdateBrush(DrawingGroup? resource, IBrush? newColorBrush)
    {
        if (resource is null)
            return null;
        if (newColorBrush is not null)
        {
            UpdateBrushes(resource);
        }

        return new DrawingImage() { Drawing = resource };

        void UpdateBrushes(DrawingGroup group)
        {
            foreach (var child in group.Children)
            {
                switch (child)
                {
                    case GeometryDrawing { Brush: ISolidColorBrush solid } geom
                        when solid.Color == Brushes.Fuchsia.Color:
                        geom.Brush = newColorBrush;
                        break;
                    case DrawingGroup childGroup:
                        UpdateBrushes(childGroup);
                        break;
                }
            }
        }
    }

    public static LinkIconConverter Instance => new();
}