using System.Collections.Immutable;

using Avalonia;
using Avalonia.Media;

using MMKiwi.ProjDash.ViewModel.Model;

namespace MMKiwi.ProjDash.GUI.Helpers;

public static class GeometryHelpers
{
    private static GeometryDrawing ToGeometry(this GeometryImport geometry, IBrush? defaultColor) =>
        new()
        {
            Geometry = PathGeometry.Parse(geometry.Path),
            Brush = (geometry is {IsForeground: true} && defaultColor is not null) || string.IsNullOrEmpty(geometry.Color)
                ? defaultColor
                : Brush.Parse(geometry.Color)
        };

    public static DrawingGroup ToDrawingGroup(this IconImport icon, IBrush? defaultColor)
    {
        DrawingCollection collection = new();
        foreach (var geometry in icon.Geometry)
        {
            collection.Add(geometry.ToGeometry(defaultColor));
        }

        return new DrawingGroup()
        {
            Children = collection,
            ClipGeometry = new RectangleGeometry(Rect.Parse(icon.ClipBounds)),
        };
    }

    public static DrawingImage ToDrawingImage(this IconImport icon, IBrush? defaultColor)
    {
        return new DrawingImage() { Drawing = icon.ToDrawingGroup(defaultColor) };
    }
}