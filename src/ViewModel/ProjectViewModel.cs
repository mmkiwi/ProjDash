// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using DynamicData;

using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace MMKiwi.ProjDash.ViewModel;

public sealed class ProjectViewModel : ViewModelBase
{
    public ProjectViewModel(Project project)
    {
        OriginalProject = project;
        Title = project.Title;
        Subtitle = string.Join(Environment.NewLine, project.Subtitles.IsDefault ? [] : project.Subtitles);
        Color = project.Color;
        _links.AddRange(project.Links.Select(p => new ProjectLinkViewModel(p)));

        IObservable<bool> isValid = _links.Connect().AutoRefresh(link => link.ValidationContext.Valid).ToCollection()
            .SelectMany(links => links.Select(link => link.ValidationContext.Valid)).Merge().Merge(ValidationContext.Valid);
        
        EditComplete = ReactiveCommand.Create(() => this, isValid);

        this.WhenActivated(d =>
        {
            this.ValidationRule(vm => vm.Title, title => !string.IsNullOrWhiteSpace(title),
                "A project title is required");
            _disposable.DisposeWith(d);
        });

        IObservable<bool> hasSelection = this.WhenAnyValue(x => x.SelectedLink).Select(x => x != null);
        DeleteLink = ReactiveCommand.Create(DeleteLinkImpl, hasSelection);
        MoveLinkDown = ReactiveCommand.Create(MoveLinkDownImpl, hasSelection);
        MoveLinkUp = ReactiveCommand.Create(MoveLinkUpImpl, hasSelection);
        AddLink = ReactiveCommand.Create(AddLinkImpl);
    }

    public Project OriginalProject { get; }

    private readonly CompositeDisposable _disposable = new();

    private readonly SourceList<ProjectLinkViewModel> _links = new();

    public string Title { get; set => this.RaiseAndSetIfChanged(ref field, value); }
    public string Subtitle { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    [field: MaybeNull]
    public ReadOnlyObservableCollection<ProjectLinkViewModel> Links
    {
        get
        {
            if (field == null)
                _links.Connect()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Bind(out field)
                    .Subscribe()
                    .DisposeWith(_disposable);
            return field;
        }
    }

    public ProjectLinkViewModel? SelectedLink { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    public string? Color { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    public Project ToProject() =>
        new()
        {
            Title = Title,
            Subtitles = [..Subtitle.Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries)],
            Color = Color,
            Links = [.._links.Items.Select(i => i.ToProjectLink()).OfType<ProjectLink>()]
        };

    public ReactiveCommand<Unit, ProjectViewModel> EditComplete { get; }
    public ReactiveCommand<Unit, Unit> DeleteLink { get; }
    private void DeleteLinkImpl() => _links.Remove(SelectedLink!);
    public ReactiveCommand<Unit, Unit> AddLink { get; }
    private void AddLinkImpl()
    {
        var newLink = new ProjectLinkViewModel();
        _links.Add(newLink);
        SelectedLink = newLink;
    }

    public ReactiveCommand<Unit, int> MoveLinkUp { get; }

    private int MoveLinkUpImpl()
    {
        var currentIndex = _links.Items.IndexOf(SelectedLink);
        _links.Move(currentIndex, --currentIndex);
        return currentIndex;
    }

    public ReactiveCommand<Unit, int> MoveLinkDown { get; }

    private int MoveLinkDownImpl()
    {
        var currentIndex = _links.Items.IndexOf(SelectedLink);
        _links.Move(currentIndex, ++currentIndex);
        return currentIndex;
    }
}