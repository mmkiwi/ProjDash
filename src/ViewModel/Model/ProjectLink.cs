namespace MMKiwi.ProjDash.ViewModel.Model;

public record ProjectLink
{
    public required string Name { get; init; }
    public UriType Type { get; init; }
    public required Uri Uri { get; init; }
    public string? Color { get; init; } 
}