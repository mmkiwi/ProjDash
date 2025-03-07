// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace MMKiwi.ProjDash.ViewModel.Model;


[ExcludeFromCodeCoverage(Justification = "DTO")]
public sealed record SettingsRoot
{
    [field: MaybeNull] public static SettingsRoot Empty => field ??= new SettingsRoot { Projects = [] };
    public required ImmutableArray<Project> Projects { get; init; }

    public IReadOnlyDictionary<string, IconImport>? IconImports { get; init; }
}