// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

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
            Brush = (geometry is { IsForeground: true } && defaultColor is not null) ||
                    string.IsNullOrEmpty(geometry.Color)
                ? defaultColor
                : Brush.Parse(geometry.Color)
        };

    private static DrawingGroup ToDrawingGroup(this IconImport icon, IBrush? defaultColor)
    {
        DrawingCollection collection = [];
        foreach (var geometry in icon.Geometry)
        {
            collection.Add(geometry.ToGeometry(defaultColor));
        }

        return new DrawingGroup
        {
            Children = collection, ClipGeometry = new RectangleGeometry(Rect.Parse(icon.ClipBounds))
        };
    }

    public static DrawingImage ToDrawingImage(this IconImport icon, IBrush? defaultColor)
    {
        return new DrawingImage { Drawing = icon.ToDrawingGroup(defaultColor) };
    }
}