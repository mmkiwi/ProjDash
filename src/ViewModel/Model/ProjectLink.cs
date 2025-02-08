namespace MMKiwi.ProjDash.ViewModel.Model;

public sealed record ProjectLink
{
    public required string Name { get; init; }
    public IconRef? Icon { get; init; }
    public required Uri Uri { get; init; }
    public string? Color { get; init; } 
}