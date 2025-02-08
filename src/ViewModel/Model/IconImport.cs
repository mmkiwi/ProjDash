using System.Collections.Immutable;

namespace MMKiwi.ProjDash.ViewModel.Model;

public sealed record IconImport
{
    public required ImmutableArray<GeometryImport> Geometry { get; init; }
    public required string ClipBounds { get; init; }
}