using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Schema;
using System.Windows.Input;

using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

using Splat;

namespace MMKiwi.ProjDash.ViewModel;

public partial class MainWindowViewModel : ViewModelBase, IEnableLogger
{
    public MainWindowViewModel()
    {
        WindowSettings = LoadWindowSettings();
        RefreshSettings = ReactiveCommand.CreateFromTask(RefreshSettingsAsync,
            outputScheduler: RxApp.MainThreadScheduler);
        SaveWindowSettings =
            ReactiveCommand.CreateFromTask<WindowSettings>(SaveWindowSettingsAsync,
                outputScheduler: RxApp.MainThreadScheduler);
        _settings = RefreshSettings
            .ToProperty(this, v => v.Settings);
        EditComplete = ReactiveCommand.CreateFromTask<ProjectViewModel>(UpdateProjectAsync);

        this.WhenActivated(d =>
        {
            this.WhenAnyValue(x => x.WindowSettings)
                .Throttle(TimeSpan.FromMilliseconds(500), RxApp.TaskpoolScheduler)
                .InvokeCommand(SaveWindowSettings)
                .DisposeWith(d);
        });
    }


    private WindowSettings LoadWindowSettings()
    {
        try
        {
            using var wndSettingsStream = File.OpenRead(WindowSettingsPath);
            {
                return JsonSerializer.Deserialize<WindowSettings>(wndSettingsStream,
                    SettingsSerializer.Default.WindowSettings);
            }
        }
        catch (Exception ex)
        {
            this.Log().Warn(ex, "Error while loading window settings");
            return default;
        }
    }

    public static string SchemaPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MMKiwi",
        "ProjDash", "Settings.schema.json");

    public static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MMKiwi",
        "ProjDash", "Settings.json");

    public static string WindowSettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MMKiwi",
        "ProjDash", "Settings.wnd.json");

    public static string LogPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MMKiwi",
        "ProjDash", "Log", "ProjDash.log");

    public ReactiveCommand<Unit, SettingsRoot> RefreshSettings { get; }
    public Interaction<ErrorDialogViewModel, Unit> ErrorDialog { get; } = new();

    public WindowSettings WindowSettings { get; set => this.RaiseAndSetIfChanged(ref field, value); }


    public ReactiveCommand<WindowSettings, Unit> SaveWindowSettings { get; }

    private async Task SaveWindowSettingsAsync(WindowSettings settings)
    {
        try
        {
            LogHost.Default.Debug($"Saving window settings {settings.Width}x{settings.Height}");
            var wndSettingsStream = File.Open(WindowSettingsPath, FileMode.Create);
            await using (wndSettingsStream.ConfigureAwait(false))
            {
                await JsonSerializer.SerializeAsync<WindowSettings>(wndSettingsStream, settings,
                    SettingsSerializer.Default.WindowSettings).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            this.Log().Warn(ex, "Error while saving window settings");
        }
    }

    private async Task<SettingsRoot> RefreshSettingsAsync()
    {
        try
        {
            this.Log().Info($"Trying to load {SettingsPath}");
            var settingsStream = File.OpenRead(SettingsPath);
            await using (settingsStream.ConfigureAwait(false))
            {
                return await JsonSerializer.DeserializeAsync<SettingsRoot>(settingsStream,
                    SettingsSerializer.Default.SettingsRoot).ConfigureAwait(false) ?? SettingsRoot.Empty;
            }
        }
        catch (Exception ex)
        {
            this.Log().Warn(ex, "Error while loading settings");
            await ErrorDialog.Handle(new ErrorDialogViewModel()
            {
                Exception = ex,
                MainMessage = $"Could not load settings from {SettingsPath}",
                PrimaryButtonText = "OK"
            });
            return SettingsRoot.Empty;
        }
    }


    private readonly ObservableAsPropertyHelper<SettingsRoot> _settings;
    public virtual SettingsRoot Settings => _settings.Value;
    public ReactiveCommand<ProjectViewModel, Unit> EditComplete { get; }

    private async Task UpdateProjectAsync(ProjectViewModel newProject)
    {
        var newSettings = Settings with { Projects = Settings.Projects.Replace(newProject.OriginalProject, newProject.ToProject()) };
        var jsonStream = File.Open(SettingsPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        await using (jsonStream.ConfigureAwait(false))
        {
            await JsonSerializer.SerializeAsync(jsonStream, newSettings, SettingsSerializer.Default.SettingsRoot).ConfigureAwait(false);
        }

        RefreshSettings.Execute().Subscribe();
    }
    
    public async Task SaveSchemaAsync()
    {
        var schemaFile = File.OpenWrite(SchemaPath);
        await using (schemaFile.ConfigureAwait(false))
        {
            using Utf8JsonWriter writer = new(schemaFile);
            var schema = SettingsSerializer.Default.SettingsRoot.GetJsonSchemaAsNode();
            schema.WriteTo(writer);
        }
    }
}

public record ErrorDialogViewModel : ReactiveRecord
{
    public required string MainMessage { get; init; }
    public Exception? Exception { get; init; }
    public bool IsExpanded { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    public required string PrimaryButtonText { get; init; }
    public string? SecondaryButtonText { get; init; }
}

public enum ErrorDialogResult
{
    Primary,
    Secondary,
}