using System.Collections.Frozen;

using MMKiwi.ProjDash.ViewModel.Model;

namespace MMKiwi.ProjDash.ViewModel;

public sealed class ProjectLinkViewModel : ViewModelBase
{
    public string Name { get; }
    public IconRef? Icon { get; }
    public Uri Uri { get;  }
    public string? Color { get; } 
    
    public IReadOnlyDictionary<string, IconImport> Icons { get; }

    public ProjectLinkViewModel(ProjectLink projectLink, IReadOnlyDictionary<string, IconImport>? icons)
    {
        Name = projectLink.Name;
        Uri = projectLink.Uri;
        Icon = projectLink.Icon;
        Color = projectLink.Color;
        Icons = icons ?? FrozenDictionary<string, IconImport>.Empty;
    }
}