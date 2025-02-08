namespace MMKiwi.ProjDash.ViewModel.Model;

public sealed record Project
{
    public required string Name { get; init; }
    public required string Client { get; init; }
    public required string ProjectNumber { get; init; }
    public required IReadOnlyList<ProjectLink> Links { get; init; }
    
    public string? Color { get; init; }
}