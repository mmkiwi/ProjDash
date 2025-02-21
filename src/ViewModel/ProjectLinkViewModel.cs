// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Reactive.Disposables;
using System.Reactive.Linq;

using MMKiwi.ProjDash.ViewModel.IconEditors;
using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;

using Splat;

namespace MMKiwi.ProjDash.ViewModel;

public sealed class ProjectLinkViewModel : ViewModelBase
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
            vector = new VectorIconViewModel(projectLink?.Icon as IconRef.ImportIcon, iconKeys.Keys);
            IconTypes = [material, file, vector];
        }
        else
            IconTypes = [material, file];


        SelectedIcon = projectLink?.Icon switch
        {
            IconRef.DataUriIcon => file,
            IconRef.ImportIcon => vector,
            IconRef.MaterialIcon => material,
            _ => null
        };

        Color = projectLink?.Color;

        this.WhenActivated(d =>
        {
            this.ValidationRule(vm => vm.Uri,
                static uri => !string.IsNullOrEmpty(uri) && System.Uri.TryCreate(uri, UriKind.Absolute, out _),
                "Link must be a valid website or file path.").DisposeWith(d);
            this.ValidationRule(vm => vm.Name,
                static name => !string.IsNullOrWhiteSpace(name),
                "Please enter a name.").DisposeWith(d);
            this.ValidationRule(
                this.WhenAnyValue(vm => vm.SelectedIcon)
                    .SelectMany(static si => si is null ? Observable.Return(false) : si.IsValid()),
                "Please select a valid icon.").DisposeWith(d);
        });
    }

    public ProjectLink? ToProjectLink()
    {
        if (Name is null || Uri is null)
            return null;
        return new ProjectLink { Name = Name, Uri = new Uri(Uri), Icon = SelectedIcon?.IconRef, Color = Color };
    }
}