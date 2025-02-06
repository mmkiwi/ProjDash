using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

using MMKiwi.ProjDash.GUI.UserControls;
using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

using Serilog;

using Splat;

namespace MMKiwi.ProjDash.GUI;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>, IEnableLogger
{
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;
        this.Hide();
        base.OnClosing(e);
    }

    public ReactiveCommand<Unit,Unit> Shutdown { get; }= ReactiveCommand.Create(() => { });
    
    public MainWindow()
    {
        PositionChanged += (s, e) => UpdatePosition(e.Point.X, e.Point.Y);
        SizeChanged += (s, e) => UpdateSize(e.NewSize.Width, e.NewSize.Height);
        Log.Verbose("MainWindow");
        if (!Design.IsDesignMode)
        {
            Shutdown.InvokeCommand((Application.Current as App)?.ExitCommand);
            EditSettingsCommand = ReactiveCommand.CreateFromTask(EditSettingsAsync);
            NewSettingsCommand = ReactiveCommand.CreateFromTask(NewSettingsAsync);
        }
        else
        {
            EditSettingsCommand =
                ReactiveCommand.Create(() => this.Log().Info($"Editing {MainWindowViewModel.SettingsPath}"));
            NewSettingsCommand =
                ReactiveCommand.Create(() => this.Log().Info($"New {MainWindowViewModel.SettingsPath}"));
            ViewModel = DesignViewModel;
        }


        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel!.ErrorDialog.RegisterHandler(HandleError);

            var settings = ViewModel!.WindowSettings;
            
            if (settings is { Width: > 0, Height: > 0 })
            {
                Width = settings.Width.Value;
                Height = settings.Height.Value;
            }
            
            if (settings is { Left: >= -10 and < 0, Top: >= -10 and < 0 })
            {
                // maximized
                WindowState = WindowState.Maximized;
            } else if (settings is { Top: not null, Left: not null })
            {
                int top = Math.Max(-8, settings.Top.Value);
                int left = Math.Max(-8, settings.Left.Value);
                Position = new PixelPoint(left, top);
            }



            _isLoaded = true;

            if (!Design.IsDesignMode)
            {
                Observable.Return(Unit.Default).InvokeCommand(ViewModel, vm => vm.RefreshSettings);
            }
        });
    }

    private void UpdateSize(double width, double height)
    {
        if (ViewModel is not null && _isLoaded)
            ViewModel.WindowSettings = ViewModel.WindowSettings with
            {
                Width = width,
                Height = height, 
            };
    }

    private void UpdatePosition(int x, int y)
    {
        if (ViewModel is not null && _isLoaded)
            ViewModel.WindowSettings = ViewModel.WindowSettings with
            {
                Top = y,
                Left = x, 
            };
    }

    private bool _isLoaded = false;


    private async Task NewSettingsAsync()
    {
        Debug.Assert(ViewModel is not null);
        await ViewModel.SaveSchemaAsync().ConfigureAwait(false);
        if (File.Exists(MainWindowViewModel.SettingsPath))
        {
            ErrorDialog dialog = new()
            {
                ViewModel = new ErrorDialogViewModel()
                {
                    MainMessage = $"The file {MainWindowViewModel.SettingsPath} already exists. Overwrite?",
                    PrimaryButtonText = "No",
                    SecondaryButtonText = "Yes"
                }
            };

            var result = await Dispatcher.UIThread.InvokeAsync(async () =>
                    await dialog.ShowDialog<ErrorDialogResult>(this).ConfigureAwait(false))
                .ConfigureAwait(false);

            if (result is not ErrorDialogResult.Secondary)
                return;
        }

        var fileStream = File.Open(MainWindowViewModel.SettingsPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        await using (fileStream.ConfigureAwait(false))
        {
            var blankRoot = new SettingsRoot()
            {
                Projects =
                [
                    new Project()
                    {
                        Name = "Name",
                        Client = "Client",
                        ProjectNumber = "000000",
                        Color = null,
                        Links =
                        [
                            new ProjectLink()
                            {
                                Name = "Link", Type = UriType.Default, Uri = new("https://example.com")
                            }
                        ]
                    }
                ]
            };

            var blankDoc = JsonSerializer.SerializeToNode(blankRoot, SettingsSerializer.Default.SettingsRoot)!;
            blankDoc["$schema"] = "Settings.schema.json";

            Utf8JsonWriter writer = new(fileStream, new JsonWriterOptions() { Indented = true });
            await using (writer.ConfigureAwait(false))
            {
                blankDoc.WriteTo(writer);
            }
        }

        await Dispatcher.UIThread.InvokeAsync(async () =>
                await Launcher.LaunchUriAsync(new Uri(MainWindowViewModel.SettingsPath)).ConfigureAwait(false))
            .ConfigureAwait(false);
    }


    private async Task EditSettingsAsync()
    {
        try
        {
            await this.Launcher.LaunchUriAsync(new Uri(MainWindowViewModel.SettingsPath)).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ErrorDialog dialog = new()
            {
                ViewModel = new ErrorDialogViewModel()
                {
                    MainMessage = $"Could not open {MainWindowViewModel.SettingsPath}",
                    PrimaryButtonText = "OK",
                    Exception = ex
                }
            };

            await dialog.ShowDialog(this).ConfigureAwait(true);
        }
    }

    public ReactiveCommand<Unit, Unit> EditSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> NewSettingsCommand { get; }

    private async Task HandleError(IInteractionContext<ErrorDialogViewModel, Unit> error)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            ErrorDialog dlg = new() { ViewModel = error.Input };
            await dlg.ShowDialog(this).ConfigureAwait(true);
            error.SetOutput(Unit.Default);
        }).ConfigureAwait(true);
    }

    public static MainWindowViewModel DesignViewModel => new(new SettingsRoot()
    {
        Projects =
        [
            ..Enumerable.Repeat(ProjectView.DesignViewModel, 10)
        ]
    });
}