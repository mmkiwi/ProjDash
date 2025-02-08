using System.Collections.Frozen;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.Model;

using Serilog;

namespace MMKiwi.ProjDash.GUI.UserControls;

public partial class ProjectView : ReactiveUserControl<ProjectViewModel>
{
    public ProjectView()
    {
        Log.Verbose("ProjectView");
        InitializeComponent();

        if (Design.IsDesignMode)
            ViewModel = new ProjectViewModel(DesignViewModel, FrozenDictionary<string, IconImport>.Empty);
    }
    
    public static Project DesignViewModel => new()
    {
        Name = "SampleProject",
        Client = "Sample Department of Transportation",
        ProjectNumber = "000000.00",
        Links =
        [
            new ProjectLink { Name = "ProjectWise", Icon = IconRef.Import("ProjectWise"), Uri = new("pw://SampleProject/") },
            new ProjectLink { Name = "VantagePoint", Icon = IconRef.Import("VantagePoint"), Uri = new("https://SampleProject/") },
            new ProjectLink { Name = "GFS", Icon = null, Uri = new("file://C:/") },
            new ProjectLink { Name = "GFS", Icon = IconRef.Material("mdi-folder-network"), Uri = new("file://C:/") },
            new ProjectLink { Name = "GFS", Icon = IconRef.Material("mdi-file-document"), Uri = new("file://C:/") }
        ]
    };
}