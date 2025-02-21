// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.GUI.Converters;
using MMKiwi.ProjDash.GUI.UserControls;
using MMKiwi.ProjDash.ViewModel;

using ReactiveUI;

using ObservableExtensions = System.ObservableExtensions;

namespace MMKiwi.ProjDash.GUI.Dialogs;

public partial class EditProjectDialog : ReactiveWindow<ProjectViewModel>
{
    public EditProjectDialog()
    {
        if (Design.IsDesignMode)
            ViewModel = new ProjectViewModel(ProjectView.DesignViewModel);

        ClearColor = ReactiveCommand.Create(() => { ViewModel!.Color = null; });

        InitializeComponent();

        this.WhenActivated(d =>
        {
            FlatTreeDataGridSource<ProjectLinkViewModel> linkSource = new(ViewModel!.Links)
            {
                Columns =
                {
                    new TemplateColumn<ProjectLinkViewModel>
                    ("", new FuncDataTemplate<ProjectLinkViewModel?>((a, _) => new Projektanker.Icons.Avalonia.Icon()
                    {
                        Value = "mdi-exclamation-thick",
                        Foreground = Brushes.Red,
                        [ToolTip.TipProperty] = "This link has errors that must be fixed.",
                        [!IsVisibleProperty] = a is null ? Observable.Return(false).ToBinding() : a.ValidationContext.Valid.Select(v=>!v).ToBinding(),
                    })),
                    new TemplateColumn<ProjectLinkViewModel>
                    ("Icon", new FuncDataTemplate<ProjectLinkViewModel?>((a, _) =>
                    {
                        var image = new Image { MaxWidth = 20, MaxHeight = 20 };
                        var imageObs = a.WhenAnyValue(vm => vm.SelectedIcon!.IconRef, vm => vm.Color)
                            .Select((i, _) =>
                                i.Item1 is null ? null : LinkIconConverter.Instance.Convert(i.Item1, i.Item2));
                        image.Bind(Image.SourceProperty, imageObs).DisposeWith(d);
                        return image;
                    })),
                    new TextColumn<ProjectLinkViewModel, string>
                        ("Name", x => x.Name),
                    new TextColumn<ProjectLinkViewModel, string>
                        ("Link", x => x.Uri)
                }
            };
            IconGrid.Source = linkSource;
            linkSource.DisposeWith(d);
            linkSource.RowSelection!.SelectionChanged += UpdateSelection;

            ObservableExtensions.Subscribe(ViewModel.MoveLinkUp, si => linkSource.RowSelection.Select(si)).DisposeWith(d);
            ObservableExtensions.Subscribe(ViewModel.MoveLinkDown, si => linkSource.RowSelection.Select(si)).DisposeWith(d);

            ObservableExtensions.Subscribe(ViewModel.EditComplete, Close).DisposeWith(d);
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