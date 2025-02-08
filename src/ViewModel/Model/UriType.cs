using System.Text.Json.Serialization;

namespace MMKiwi.ProjDash.ViewModel.Model;

[JsonConverter(typeof(JsonStringEnumConverter<UriType>))]
public enum UriType
{
    Default,
    Website,
    VantagePoint,
    ProjectWise,
    GFS,
    File
}