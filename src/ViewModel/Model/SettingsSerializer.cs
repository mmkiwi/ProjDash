// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace MMKiwi.ProjDash.ViewModel.Model;

[JsonSerializable(typeof(SettingsRoot))]
[JsonSerializable(typeof(WindowSettings))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals)]
public sealed partial class SettingsSerializer : JsonSerializerContext;