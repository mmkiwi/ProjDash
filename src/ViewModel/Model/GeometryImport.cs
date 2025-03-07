// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;

namespace MMKiwi.ProjDash.ViewModel.Model;

[ExcludeFromCodeCoverage(Justification = "DTO")]
public sealed record GeometryImport
{
    public required string Path { get; init; }
    public string? Color { get; init; } = "";
    public bool IsForeground { get; init; }
}