using System.Text.Json.Serialization;

namespace MMKiwi.ProjDash.ViewModel.Model;

[JsonDerivedType(typeof(MaterialIcon), "material")]
[JsonDerivedType(typeof(ImportIcon), "import")]
[JsonDerivedType(typeof(DataUriIcon), "dataUri")]
public abstract class IconRef
{
    private IconRef() { }
    public required string Reference { get; init; }

    public sealed class MaterialIcon : IconRef;

    public sealed class ImportIcon : IconRef;
    public sealed class DataUriIcon : IconRef;
    
    public static MaterialIcon Material(string reference) => new() { Reference = reference };
    public static ImportIcon Import(string reference) => new() { Reference = reference };
    public static DataUriIcon DataUri(string reference) => new() { Reference = reference };
}