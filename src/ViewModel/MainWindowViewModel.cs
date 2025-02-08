using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Schema;

using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

using Splat;

namespace MMKiwi.ProjDash.ViewModel;

public partial class MainWindowViewModel : ViewModelBase, IEnableLogger
{
    public MainWindowViewModel(SettingsRoot settings)
    {
        RefreshSettings = ReactiveCommand.Create<Unit, SettingsRoot>(_ => SettingsRoot.Empty);
        SaveWindowSettings = ReactiveCommand.Create<WindowSettings, Unit>(_ => Unit.Default);
        // this is design, just set settings
        _settings = Observable.Return(settings)
            .ToProperty(this, v => v.Settings);
    }

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
    public SettingsRoot Settings => _settings.Value;

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