using Avalonia.Controls;
using Avalonia.Controls.Templates;

using MMKiwi.ProjDash.GUI.UserControls;
using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.Model;

namespace MMKiwi.ProjDash.GUI;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        return data switch
        {
            MainWindowViewModel mainWindowViewModel => new MainWindow()
            {
                ViewModel = mainWindowViewModel
            },
            Project project => new ProjectView()
            {
                DataContext = project
            },
            ProjectLink projectLink => new ProjectLinkButton()
            {
                ViewModel = projectLink
            },
            _ => new TextBlock { Text = $"Not Found: {data?.GetType().Name ?? null}" }
        };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase or Project or ProjectLink;
    }
}