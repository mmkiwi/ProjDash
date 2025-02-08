using MMKiwi.ProjDash.ViewModel.Model;

namespace MMKiwi.ProjDash.ViewModel;

public sealed class ProjectViewModel: ViewModelBase
{
    public ProjectViewModel(Project project, IReadOnlyDictionary<string, IconImport>? icons)
    {
        Name = project.Name;
        Client = project.Client;
        ProjectNumber = project.ProjectNumber;
        Color = project.Color;
        Links = [.. project.Links.Select(p => new ProjectLinkViewModel(p, icons))];
    }

    public string Name { get; }
    public string Client { get; }
    public string ProjectNumber { get; }
    public IReadOnlyList<ProjectLinkViewModel> Links { get; }

    public string? Color { get; }
}