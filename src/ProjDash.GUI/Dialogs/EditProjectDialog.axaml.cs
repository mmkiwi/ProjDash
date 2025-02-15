using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using DynamicData.Binding;
using DynamicData;

using MMKiwi.ProjDash.GUI.Converters;
using MMKiwi.ProjDash.GUI.UserControls;
using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

namespace MMKiwi.ProjDash.GUI.Dialogs;

public partial class EditProjectDialog : ReactiveWindow<ProjectViewModel>
{
    public EditProjectDialog()
    {
        if (Design.IsDesignMode)
            ViewModel = new ProjectViewModel(Guid.CreateVersion7(), ProjectView.DesignViewModel);

        ClearColor = ReactiveCommand.Create(() => { ViewModel!.Color = null; });

        InitializeComponent();

        this.WhenActivated(d =>
        {
            FlatTreeDataGridSource<ProjectLinkViewModel> linkSource = new(ViewModel!.Links)
            {
                Columns =
                {
                    new TemplateColumn<ProjectLinkViewModel>
                    ("Icon", new FuncDataTemplate<ProjectLinkViewModel?>((a, e) =>
                    {
                        var image = new Image() { MaxWidth = 20, MaxHeight = 20 };
                        var imageObs = a.WhenAnyValue(vm => vm.SelectedIcon!.IconRef, vm => vm.Color)
                            .Select((i, _) =>
                                i.Item1 is null ? null : LinkIconConverter.Instance.Convert(i.Item1, i.Item2));
                        image.Bind(Image.SourceProperty, imageObs).DisposeWith(d);
                        return image;
                    })),
                    new TextColumn<ProjectLinkViewModel, string>
                        ("Name", x => x.Name),
                    new TextColumn<ProjectLinkViewModel, string>
                        ("Link", x => x.Uri),
                },
            };
            IconGrid.Source = linkSource;
            linkSource.DisposeWith(d);
            linkSource.RowSelection!.SelectionChanged += UpdateSelection;

            ViewModel.MoveLinkUp.Subscribe(si => linkSource.RowSelection.Select(si)).DisposeWith(d);
            ViewModel.MoveLinkDown.Subscribe(si => linkSource.RowSelection.Select(si)).DisposeWith(d);

            ViewModel.EditComplete.Subscribe(Close).DisposeWith(d);
        });
    }

    private void UpdateSelection(object? sender, TreeSelectionModelSelectionChangedEventArgs<ProjectLinkViewModel> e)
    {
        ViewModel!.SelectedLink = e.SelectedItems switch
        {
            { Count: > 0 } => e.SelectedItems[0],
            _ => null
        };
    }

    public ReactiveCommand<Unit, Unit> ClearColor { get; }


}