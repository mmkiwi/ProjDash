using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.IconEditors;
using MMKiwi.ProjDash.ViewModel.Model;

using Projektanker.Icons.Avalonia.MaterialDesign;

using ReactiveUI;

using Serilog;

namespace MMKiwi.ProjDash.GUI.UserControls;

public partial class MaterialIconEditor : ReactiveUserControl<MaterialIconViewModel>
{
    public MaterialIconEditor()
    {
        InitializeComponent();
    }

    private static IEnumerable<MaterialIconListItem> GetIconNames()
    {
        foreach (string r in MdiAssembly.GetManifestResourceNames())
        {
            Match match = ResourceRegex.Match(r);
            if (match.Success)
            {
                var shortName = match.Groups["id"].Value;
                yield return new MaterialIconListItem(shortName,
                    new IconRef.MaterialIcon() { Reference = "mdi-" + shortName, });
            }
            else
            {
                Log.Verbose($"Could not load icon {r}");
            }
        }
    }

    public void LaunchIconList()
    {
        TopLevel.GetTopLevel(this)?.Launcher.LaunchUriAsync(new("https://pictogrammers.com/library/mdi/"));
    }

    private static readonly Lazy<IReadOnlyList<MaterialIconListItem>> _iconNames = new(() => [..GetIconNames()]);
    public static IReadOnlyList<MaterialIconListItem> IconNames => _iconNames.Value;

    private static readonly Assembly MdiAssembly = typeof(MaterialDesignIconProvider).Assembly;

    [GeneratedRegex(@"Projektanker\.Icons\.Avalonia\.MaterialDesign\.Assets\.(?<id>[-a-zA-Z0-9]+)\.svg")]
    private static partial Regex ResourceRegex { get; }
}

public record MaterialIconListItem(string ShortName, IconRef.MaterialIcon Icon);