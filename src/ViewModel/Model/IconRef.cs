// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MMKiwi.ProjDash.ViewModel.Model;

[JsonDerivedType(typeof(MaterialIcon), "material")]
[JsonDerivedType(typeof(ImportIcon), "import")]
[JsonDerivedType(typeof(DataUriIcon), "dataUri")]
public abstract record IconRef
{
    [ExcludeFromCodeCoverage]
    private IconRef() { }
    public required string Reference { get; init; }

    public sealed record MaterialIcon : IconRef;

    public sealed record ImportIcon : IconRef;

    public sealed record DataUriIcon : IconRef;

    public static MaterialIcon Material(string reference) => new() { Reference = reference };
    public static ImportIcon Import(string reference) => new() { Reference = reference };
    public static DataUriIcon DataUri(string reference) => new() { Reference = reference };
}