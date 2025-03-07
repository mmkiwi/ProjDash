// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;

namespace MMKiwi.ProjDash.ViewModel.Model;

[ExcludeFromCodeCoverage(Justification = "DTO")]
public readonly record struct WindowSettings
{
    public required double? Width { get; init; }
    public required double? Height { get; init; }
    public required int? Left { get; init; }
    public required int? Top { get; init; }
}