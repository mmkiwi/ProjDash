using System.Diagnostics.CodeAnalysis;

namespace MMKiwi.ProjDash.ViewModel.Model;

public record SettingsRoot
{
    [field:MaybeNull]
    public static SettingsRoot Empty => field ??= new SettingsRoot() { Projects = [] };
    public required IReadOnlyList<Project> Projects { get; init; }
}

public readonly record struct WindowSettings
{
    public required double? Width { get; init; }
    public required double? Height { get; init; }
    public required int? Left { get; init; }
    public required int? Top { get; init; }
}