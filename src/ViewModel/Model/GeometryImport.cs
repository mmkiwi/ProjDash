namespace MMKiwi.ProjDash.ViewModel.Model;

public sealed record GeometryImport
{
    public required string Path { get; init; }
    public string? Color { get; init; } = "";
    public bool IsForeground { get; init; } = false;
}