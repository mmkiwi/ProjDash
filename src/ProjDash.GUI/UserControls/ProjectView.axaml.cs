using System.Collections.Frozen;
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

using Splat;

namespace MMKiwi.ProjDash.GUI.UserControls;

public partial class ProjectView : ReactiveUserControl<Project>
{
    public ProjectView()
    {
        MainWindow mainWindow = App.Current!.MainWindow!;
        Log.Verbose("ProjectView");
        if (Design.IsDesignMode)
        {
            ViewModel = DesignViewModel;
            Edit = ReactiveCommand.Create<ProjectViewModel?>(() => null);
            Delete = ReactiveCommand.Create(() => { });
        }
        else
        {
            Edit = ReactiveCommand.CreateFromTask(EditAsync);
            Delete = ReactiveCommand.Create(() => { });

            this.WhenActivated(d =>
            {
                Edit.Where(res => res is not null).InvokeCommand(mainWindow.ViewModel!.EditComplete!).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Subtitles, v => v.Subtitle.Text,
                        subtitle => string.Join(Environment.NewLine, subtitle))
                    .DisposeWith(d);
            });
            
        }
        
        InitializeComponent();
    }

    public ReactiveCommand<Unit, ProjectViewModel?> Edit { get; }

    private async Task<ProjectViewModel?> EditAsync()
    {
        EditProjectDialog dialog = new() { ViewModel = new ProjectViewModel(Guid.CreateVersion7(), ViewModel!) };
        return await dialog.ShowDialog<ProjectViewModel?>((Window)TopLevel.GetTopLevel(this)!).ConfigureAwait(true);
    }

    public ReactiveCommand<Unit, Unit> Delete { get; }

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
                Name = "ProjectWise", Icon = IconRef.Import("ProjectWise"), Uri = new("pw://SampleProject/")
            },
            new ProjectLink
            {
                Name = "VantagePoint", Icon = IconRef.Import("VantagePoint"), Uri = new("https://SampleProject/")
            },
            new ProjectLink { Name = "GFS", Icon = null, Uri = new("file://C:/Temp/Test1") },
            new ProjectLink { Name = "GFS", Icon = IconRef.Material("mdi-folder-network"), Uri = new("file://C:/Temp/Test2") },
            new ProjectLink { Name = "GFS", Icon = IconRef.Material("mdi-file-document"), Uri = new("file://C:/Temp/Test3") }
        ]
    };
}