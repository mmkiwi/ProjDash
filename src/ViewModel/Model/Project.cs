using System.Collections.Immutable;

namespace MMKiwi.ProjDash.ViewModel.Model;

public sealed record Project
{
    public required string Title { get; init; }
    public ImmutableArray<string> Subtitles { get; init; } = [];
    public required ImmutableArray<ProjectLink> Links { get; init; }
    
    public string? Color { get; init; }
}