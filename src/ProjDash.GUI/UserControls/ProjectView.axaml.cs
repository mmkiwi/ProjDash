// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.GUI.Dialogs;
using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

using Serilog;

namespace MMKiwi.ProjDash.GUI.UserControls;

public partial class ProjectView : ReactiveUserControl<Project>
{
    public ProjectView()
    {
        MainWindow mainWindow = App.Current!.MainWindow!;
        Log.Verbose("ProjectView");

        Delete = ReactiveCommand.Create(() => ViewModel);
        if (Design.IsDesignMode)
        {
            ViewModel = DesignViewModel;
            Edit = ReactiveCommand.Create<ProjectViewModel?>(() => null);
            Add = ReactiveCommand.Create<ProjectViewModel?>(() => null);
        }
        else
        {
            Edit = ReactiveCommand.CreateFromTask(EditAsync);
            Add = mainWindow.Add;

            this.WhenActivated(d =>
            {
                Edit.Where(res => res is not null).InvokeCommand(mainWindow.ViewModel!.EditComplete!).DisposeWith(d);
                Delete.InvokeCommand(mainWindow.ViewModel!.DeleteProject);
                this.OneWayBind(ViewModel, vm => vm.Subtitles, v => v.Subtitle.Text,
                        subtitles => string.Join(Environment.NewLine, subtitles.IsDefault ? [] : subtitles))
                    .DisposeWith(d);
            });
        }

        InitializeComponent();
    }

    public ReactiveCommand<Unit, ProjectViewModel?> Edit { get; }

    private async Task<ProjectViewModel?> EditAsync()
    {
        EditProjectDialog dialog = new() { ViewModel = new ProjectViewModel(ViewModel!) };
        return await dialog.ShowDialog<ProjectViewModel?>((Window)TopLevel.GetTopLevel(this)!).ConfigureAwait(true);
    }

    public ReactiveCommand<Unit, ProjectViewModel?> Add { get; }
    public ReactiveCommand<Unit, Project?> Delete { get; }

    public static Project DesignViewModel => new()
    {
        Title = "SampleProject",
        Subtitles =
        [
            "Sample Department of Transportation",
            "000000.00"
        ],
        Links =
        [
            new ProjectLink
            {
                Name = "ProjectWise",
                Icon = IconRef.Import("ProjectWise"),
                Uri = new Uri(
                    "pw://SampleProject/this/is/a/really/long/url/that/makes/things/extend/past/the/width/of/the/window")
            },
            new ProjectLink
            {
                Name = "VantagePoint",
                Icon = IconRef.Import("VantagePoint"),
                Uri = new Uri("https://SampleProject/")
            },
            new ProjectLink { Name = "GFS", Icon = null, Uri = new Uri("file://C:/Temp/Test1") },
            new ProjectLink
            {
                Name = "GFS", Icon = IconRef.Material("mdi-folder-network"), Uri = new Uri("file://C:/Temp/Test2")
            },
            new ProjectLink
            {
                Name = "GFS", Icon = IconRef.Material("mdi-file-document"), Uri = new Uri("file://C:/Temp/Test3")
            }
        ]
    };
}