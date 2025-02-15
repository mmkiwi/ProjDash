using System.Collections.Frozen;
using System.Reactive.Disposables;

using MMKiwi.ProjDash.ViewModel.IconEditors;
using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;

using Splat;

namespace MMKiwi.ProjDash.ViewModel;

public sealed class ProjectLinkViewModel : ViewModelBase, IValidatableViewModel
{
    public string? Name { get; set => this.RaiseAndSetIfChanged(ref field, value); }
    public string? Uri { get; set => this.RaiseAndSetIfChanged(ref field, value); }
    public string? Color { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    public IReadOnlyList<IconViewModel> IconTypes { get; init; }

    public IconViewModel? SelectedIcon { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    private static string? PrettyUriFormat(Uri? uri) =>
        uri switch
        {
            { IsFile: true } => uri.LocalPath,
            null => null,
            _ => uri.ToString()
        };

    public ProjectLinkViewModel(ProjectLink? projectLink = null)
    {
        Name = projectLink?.Name;
        Uri = PrettyUriFormat(projectLink?.Uri);

        MaterialIconViewModel material = new(projectLink?.Icon as IconRef.MaterialIcon);
        FileIconViewModel file = new(projectLink?.Icon as IconRef.DataUriIcon);

        VectorIconViewModel? vector = null;
        if (Locator.Current.GetService<MainWindowViewModel>()?.Settings.IconImports is { Count: > 0 } iconKeys)
        {
            vector = new(projectLink?.Icon as IconRef.ImportIcon, iconKeys.Keys);
            IconTypes = [material, file, vector];
        }
        else 
            IconTypes = [material, file];


        SelectedIcon = projectLink?.Icon switch
        {
            IconRef.DataUriIcon dataUriIcon => file,
            IconRef.ImportIcon importIcon => vector,
            IconRef.MaterialIcon materialIcon => material,
            _ => null
        };

        Color = projectLink?.Color;

        this.WhenActivated(d =>
        {
            this.ValidationRule(vm => vm.Uri,
                uri => System.Uri.TryCreate(uri, UriKind.Absolute, out _),
                "Link must be a valid website or file path.").DisposeWith(d);
            this.ValidationRule(vm => vm.Name,
                name => !string.IsNullOrWhiteSpace(name),
                "Please enter a name.").DisposeWith(d);
        });
    }

    public ProjectLink? ToProjectLink()
    {
        if (Name is null || Uri is null)
            return null;
        return new ProjectLink()
        {
            Name = Name, Uri = new(Uri), Icon = SelectedIcon?.IconRef, Color = Color,
        };
    }

    public IValidationContext ValidationContext { get; } = new ValidationContext();
}