using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

using Serilog;

namespace MMKiwi.ProjDash.GUI.UserControls;

public partial class ProjectLinkEditor : ReactiveUserControl<ProjectLinkViewModel>
{
    public ProjectLinkEditor()
    {
        if (Design.IsDesignMode)
            ViewModel = new(new ProjectLink()
            {
                Name = "Test Link",
                Uri = new("http://example.com"),
                Icon = new IconRef.MaterialIcon()
                {
                    Reference = "close"
                }
            });
        InitializeComponent();

        this.WhenActivated(d =>
        {
            if (ViewModel is null)
                return;
            ViewModel.WhenActivated(d);
        });
    }
}

