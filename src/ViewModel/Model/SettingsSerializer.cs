using System.Text.Json.Serialization;

namespace MMKiwi.ProjDash.ViewModel.Model;

[JsonSerializable(typeof(SettingsRoot))]
[JsonSerializable(typeof(WindowSettings))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class SettingsSerializer : JsonSerializerContext;