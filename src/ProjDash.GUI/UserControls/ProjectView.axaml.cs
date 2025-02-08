using Avalonia.Controls;

using MMKiwi.ProjDash.ViewModel.Model;

using Serilog;

namespace MMKiwi.ProjDash.GUI.UserControls;

public partial class ProjectView : UserControl
{
    public ProjectView()
    {
        Log.Verbose("ProjectView");
        InitializeComponent();
    }
    
    public static Project DesignViewModel => new Project()
    {
        Name = "SampleProject",
        Client = "Sample Department of Transportation",
        ProjectNumber = "000000.00",
        Links =
        [
            new ProjectLink { Name = "ProjectWise", Type = UriType.ProjectWise, Uri = new("pw://SampleProject/") },
            new ProjectLink { Name = "VantagePoint", Type = UriType.VantagePoint, Uri = new("https://SampleProject/") },
            new ProjectLink { Name = "GFS", Type = UriType.Default, Uri = new("file://C:/") },
            new ProjectLink { Name = "GFS", Type = UriType.GFS, Uri = new("file://C:/") },
            new ProjectLink { Name = "GFS", Type = UriType.Default, Uri = new("file://C:/") },
            new ProjectLink { Name = "GFS", Type = UriType.Website, Uri = new("file://C:/") }
        ]
    };
}