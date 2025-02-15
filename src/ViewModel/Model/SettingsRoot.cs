using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace MMKiwi.ProjDash.ViewModel.Model;

public sealed  record SettingsRoot
{
    [field:MaybeNull]
    public static SettingsRoot Empty => field ??= new SettingsRoot() { Projects = [] };
    public required ImmutableArray<Project> Projects { get; init; }
    
    public IReadOnlyDictionary<string, IconImport>? IconImports { get; init; }
}