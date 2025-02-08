using System.Text.Json.Serialization;

namespace MMKiwi.ProjDash.ViewModel.Model;

[JsonDerivedType(typeof(MaterialIcon), "material")]
[JsonDerivedType(typeof(ImportIcon), "import")]
public abstract class IconRef
{
    private IconRef() { }
    public required string Reference { get; init; }

    public sealed class MaterialIcon : IconRef;

    public sealed class ImportIcon : IconRef;
    
    public static MaterialIcon Material(string reference) => new MaterialIcon { Reference = reference };
    public static ImportIcon Import(string reference) => new ImportIcon { Reference = reference };
}