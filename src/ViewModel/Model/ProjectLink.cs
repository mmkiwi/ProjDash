namespace MMKiwi.ProjDash.ViewModel.Model;

public record ProjectLink
{
    public required string Name { get; init; }
    public required UriType Type { get; init; }
    public required Uri Uri { get; init; }
}