// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace MMKiwi.ProjDash.ViewModel.Model;

[ExcludeFromCodeCoverage(Justification = "DTO")]
public sealed record IconImport
{
    public required ImmutableArray<GeometryImport> Geometry { get; init; }
    public required string ClipBounds { get; init; }
}