using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using DynamicData;

using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

namespace MMKiwi.ProjDash.ViewModel;

public sealed class ProjectViewModel : ViewModelBase
{
    public ProjectViewModel(Guid id, Project project)
    {
        OriginalProject = project;
        Id = id;
        Title = project.Title;
        Subtitle = string.Join(Environment.NewLine, project.Subtitles);
        Color = project.Color;
        _links.AddRange(project.Links.Select(p => new ProjectLinkViewModel(p)));

        EditComplete = ReactiveCommand.Create(() => this);
        Delete = ReactiveCommand.Create(() => this);

        this.WhenActivated(d => _disposable.DisposeWith(d));

        IObservable<bool> hasSelection = this.WhenAnyValue(x => x.SelectedLink).Select(x => x != null);
        DeleteLink = ReactiveCommand.Create(DeleteLinkImpl, hasSelection);
        MoveLinkDown = ReactiveCommand.Create(MoveLinkDownImpl, hasSelection);
        MoveLinkUp = ReactiveCommand.Create(MoveLinkUpImpl, hasSelection);
        AddLink = ReactiveCommand.Create(AddLinkImpl);
    }

    public Project OriginalProject { get; }

    public Guid Id { get; }

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
    public ReactiveCommand<Unit, ProjectViewModel> Delete { get; }

    public ReactiveCommand<Unit, Unit> DeleteLink { get; }
    private void DeleteLinkImpl() => _links.Remove(SelectedLink!);
    public ReactiveCommand<Unit, Unit> AddLink { get; }
    private void AddLinkImpl() => _links.Add(new());

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

    public ProjectViewModel? Clone() => new(Id, ToProject());

    public IObservable<IChangeSet<ProjectLinkViewModel>> ConnectLinks()
    {
        return _links.Connect();
    }
}