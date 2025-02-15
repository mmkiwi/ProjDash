using Avalonia.Controls;
using Avalonia.Controls.Templates;

using MMKiwi.ProjDash.GUI.Dialogs;
using MMKiwi.ProjDash.GUI.UserControls;
using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.IconEditors;
using MMKiwi.ProjDash.ViewModel.Model;

namespace MMKiwi.ProjDash.GUI;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        return data switch
        {
            Project project => new ProjectView()
            {
                ViewModel = project
            },
            MainWindowViewModel mainWindowViewModel => new MainWindow()
            {
                ViewModel = mainWindowViewModel
            },
            ProjectLink projectLink => new ProjectLinkButton()
            {
                ViewModel = projectLink
            },
            ProjectViewModel projectVm => new EditProjectDialog()
            {
                ViewModel = projectVm
            },
            ProjectLinkViewModel projectLink => new ProjectLinkEditor()
            {
                ViewModel = projectLink
            },
            MaterialIconViewModel material => new MaterialIconEditor()
            {
                ViewModel = material
            },
            FileIconViewModel material => new FileIconEditor()
            {
                ViewModel = material
            },
            VectorIconViewModel vector => new VectorIconEditor()
            {
                ViewModel = vector
            },
            _ => new TextBlock { Text = $"Not Found: {data?.GetType().Name ?? null}" }
        };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase or Project or ProjectLink;
    }
}