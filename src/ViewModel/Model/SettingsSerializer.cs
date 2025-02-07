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
public partial class SettingsSerializer : JsonSerializerContext;