using System.Globalization;

using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

using MMKiwi.ProjDash.ViewModel.Model;

namespace MMKiwi.ProjDash.GUI.Converters;

public class LinkIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is UriType uriType && targetType.IsAssignableTo(typeof(IImage)))
        {
            return Convert(uriType);
        }

        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(),
            BindingErrorType.Error);
    }

    public IImage? Convert(UriType uriType)
    {
        return uriType switch
        {
            UriType.VantagePoint => Application.Current?.Resources["VantagePointImage"] as IImage,
            UriType.GFS => new Projektanker.Icons.Avalonia.IconImage() { Value = "mdi-folder-network" },
            UriType.File => new Projektanker.Icons.Avalonia.IconImage() { Value = "mdi-folder" },
            UriType.ProjectWise =>
                Application.Current?.Resources["ProjectWiseImage"] as IImage,
            _ => new Projektanker.Icons.Avalonia.IconImage() { Value = "mdi-link" }
        };
    }
    
    public static LinkIconConverter Instance => new();

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}