// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Frozen;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;

using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

using MMKiwi.ProjDash.GUI.Dialogs;
using MMKiwi.ProjDash.GUI.UserControls;
using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.Model;
using MMKiwi.ProjDash.ViewModel.Services;

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

    public ReactiveCommand<Unit, Unit> Shutdown { get; } = ReactiveCommand.Create(() => { });

    public MainWindow()
    {
        IProjDashSettingsService? settingsService = Locator.Current.GetService<IProjDashSettingsService>();
        PositionChanged += (_, e) => UpdatePosition(e.Point.X, e.Point.Y);
        SizeChanged += (_, e) => UpdateSize(e.NewSize.Width, e.NewSize.Height);
        Log.Verbose("MainWindow");
        if (!Design.IsDesignMode)
        {
            Shutdown.InvokeCommand((Application.Current as App)?.ExitCommand);
            EditSettingsCommand = ReactiveCommand.CreateFromTask(EditSettingsAsync);
            NewSettingsCommand = ReactiveCommand.CreateFromTask(NewSettingsAsync);
            Add = ReactiveCommand.CreateFromTask(AddAsync);
            
            
            

            ShowLogsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                string? path = Path.GetDirectoryName(Program.LogPath);
                if (Directory.Exists(path))
                    await Launcher.LaunchUriAsync(new Uri(path)).ConfigureAwait(false);
            });
        }
        else
        {
            EditSettingsCommand =
                ReactiveCommand.Create(() => Log.Information($"Editing {settingsService?.SettingsPath}"));
            NewSettingsCommand =
                ReactiveCommand.Create(() => Log.Information($"New {settingsService?.SettingsPath}"));
            ShowLogsCommand =
                ReactiveCommand.Create(() => Log.Information("Showing Logs"));
            Add =
                ReactiveCommand.Create<ProjectViewModel?>(static () =>
                {
                    Log.Information("Showing Logs");
                    return null;
                });
            ViewModel = new DesignMainWindowViewModel();
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

            switch (settings)
            {
                case { Left: >= -10 and < 0, Top: >= -10 and < 0 }:
                    // maximized
                    WindowState = WindowState.Maximized;
                    break;
                case { Top: not null, Left: not null }:
                    {
                        int top = Math.Max(-8, settings.Top.Value);
                        int left = Math.Max(-8, settings.Left.Value);
                        Position = new PixelPoint(left, top);
                        break;
                    }
            }

            _isLoaded = true;

            Add.Where(res => res is not null).InvokeCommand(ViewModel!.AddProject!).DisposeWith(d);
            
            if (!Design.IsDesignMode)
            {
                // Hook the message to show a window 
                WindowPipe.ShowWindowMessage.SubscribeOn(RxApp.MainThreadScheduler).Subscribe(_ =>
                {
                    Log.Information("Bring to front message recieved");
                    App.Current!.Show();
                }).DisposeWith(d);

                Observable.Return(Unit.Default).InvokeCommand(ViewModel, vm => vm.RefreshSettings);
            }
        });
    }

    public ReactiveCommand<Unit, ProjectViewModel?> Add { get; }

    private async Task<ProjectViewModel?> AddAsync()
    {
        EditProjectDialog dialog = new()
        {
            ViewModel = new ProjectViewModel(new Project { Title = "New Project", Links = [] })
        };
        return await dialog.ShowDialog<ProjectViewModel?>((Window)TopLevel.GetTopLevel(this)!).ConfigureAwait(true);
    }

    private void UpdateSize(double width, double height)
    {
        if (ViewModel is not null && _isLoaded)
            ViewModel.WindowSettings = ViewModel.WindowSettings with { Width = width, Height = height };
    }

    private void UpdatePosition(int x, int y)
    {
        if (ViewModel is not null && _isLoaded)
            ViewModel.WindowSettings = ViewModel.WindowSettings with { Top = y, Left = x };
    }

    private bool _isLoaded;

    #warning todo move up down
    
    private async Task NewSettingsAsync()
    {
        IProjDashSettingsService? settingsService = Locator.Current.GetService<IProjDashSettingsService>()!;
        Debug.Assert(ViewModel is not null);
        await ViewModel.SaveSchemaAsync().ConfigureAwait(false);
        if (File.Exists(settingsService?.SettingsPath))
        {
            ErrorDialog dialog = new()
            {
                ViewModel = new ErrorDialogViewModel
                {
                    MainMessage = $"The file {settingsService?.SettingsPath} already exists. Overwrite?",
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

        var fileStream = File.Open(settingsService.SettingsPath!, FileMode.Create, FileAccess.Write, FileShare.Read);
        await using (fileStream.ConfigureAwait(false))
        {
            var blankRoot = new SettingsRoot
            {
                Projects =
                [
                    new Project
                    {
                        Title = "Name",
                        Subtitles = ["Client", "000000"],
                        Color = null,
                        Links =
                        [
                            new ProjectLink
                            {
                                Name = "Link",
                                Icon = IconRef.Material("mdi-folder"),
                                Uri = new Uri("https://example.com")
                            },
                            new ProjectLink
                            {
                                Name = "Link",
                                Icon = IconRef.Import("rectangle"),
                                Uri = new Uri("https://example.com")
                            }
                        ]
                    }
                ],
                IconImports = new Dictionary<string, IconImport>
                {
                    {
                        "rectangle", new IconImport
                        {
                            ClipBounds = "0.0,0.0,256.0,256.0",
                            Geometry =
                            [
                                new GeometryImport
                                {
                                    Path = "M 0,0 H 256 V 256 H 0 Z", Color = "yellow", IsForeground = true
                                }
                            ]
                        }
                    }
                }.ToFrozenDictionary()
            };

            var blankDoc = JsonSerializer.SerializeToNode(blankRoot, SettingsSerializer.Default.SettingsRoot)!;
            blankDoc["$schema"] = "Settings.schema.json";

            Utf8JsonWriter writer = new(fileStream, new JsonWriterOptions { Indented = true });
            await using (writer.ConfigureAwait(false))
            {
                blankDoc.WriteTo(writer);
            }
        }

        await Dispatcher.UIThread.InvokeAsync(async () =>
                await Launcher.LaunchUriAsync(new Uri(settingsService.SettingsPath)).ConfigureAwait(false))
            .ConfigureAwait(false);
    }


    private async Task EditSettingsAsync()
    {
        IProjDashSettingsService settingsService = Locator.Current.GetService<IProjDashSettingsService>()!;
        try
        {
            await ViewModel!.SaveSchemaAsync().ConfigureAwait(true);
            await this.Launcher.LaunchUriAsync(new Uri(settingsService.SettingsPath)).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ErrorDialog dialog = new()
            {
                ViewModel = new ErrorDialogViewModel
                {
                    MainMessage = $"Could not open {settingsService.SettingsPath}",
                    PrimaryButtonText = "OK",
                    Exception = ex
                }
            };

            await dialog.ShowDialog(this).ConfigureAwait(true);
        }
    }

    public ReactiveCommand<Unit, Unit> EditSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> NewSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowLogsCommand { get; }

    private async Task HandleError(IInteractionContext<ErrorDialogViewModel, Unit> error)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            ErrorDialog dlg = new() { ViewModel = error.Input };
            await dlg.ShowDialog(this).ConfigureAwait(true);
            error.SetOutput(Unit.Default);
        }).ConfigureAwait(true);
    }
}

public class DesignMainWindowViewModel : MainWindowViewModel
{
    public override SettingsRoot Settings =>
        new()
        {
            Projects =
            [
                new Project
                {
                    Title = "Name",
                    Subtitles = ["", null!],
                    Color = null,
                    Links =
                    [
                        new ProjectLink
                        {
                            Name = "Link",
                            Icon = IconRef.Material("mdi-folder"),
                            Uri = new Uri("https://example.com")
                        },
                        new ProjectLink
                        {
                            Name = "Link", Icon = IconRef.Import("rectangle"), Uri = new Uri("https://example.com")
                        }
                    ]
                },
                ..Enumerable.Repeat(ProjectView.DesignViewModel, 10)
            ]
        };
}