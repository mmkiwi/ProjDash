namespace MMKiwi.ProjDash.ViewModel.Model;

public readonly record struct WindowSettings
{
    public required double? Width { get; init; }
    public required double? Height { get; init; }
    public required int? Left { get; init; }
    public required int? Top { get; init; }
}