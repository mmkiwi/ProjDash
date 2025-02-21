// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MMKiwi.ProjDash.ViewModel.Model;

public sealed record ProjectLink
{
    public required string Name { get; init; }
    public IconRef? Icon { get; init; }
    public required Uri Uri { get; init; }
    public string? Color { get; init; }
}