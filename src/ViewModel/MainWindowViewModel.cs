// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Frozen;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Schema;

using MMKiwi.ProjDash.ViewModel.Model;
using MMKiwi.ProjDash.ViewModel.Services;

using ReactiveUI;

using Splat;

namespace MMKiwi.ProjDash.ViewModel;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        SettingsService = Locator.Current.GetService<IProjDashSettingsService>() ?? new FileProjDashSettingsService();
        WindowSettings = LoadWindowSettings();
        RefreshSettings = ReactiveCommand.CreateFromTask(RefreshSettingsAsync,
            outputScheduler: RxApp.MainThreadScheduler);
        SaveWindowSettings =
            ReactiveCommand.CreateFromTask<WindowSettings>(SaveWindowSettingsAsync,
                outputScheduler: RxApp.MainThreadScheduler);
        _settings = RefreshSettings
            .ToProperty(this, v => v.Settings);
        EditComplete = ReactiveCommand.CreateFromTask<ProjectViewModel>(UpdateProjectAsync);

        AddProject = ReactiveCommand.CreateFromTask<ProjectViewModel>(AddProjectAsync);
        DeleteProject = ReactiveCommand.CreateFromTask<Project?>(DeleteProjectImpl);

        this.WhenActivated(d =>
        {
            this.WhenAnyValue(x => x.WindowSettings)
                .Throttle(TimeSpan.FromMilliseconds(500), RxApp.TaskpoolScheduler)
                .InvokeCommand(SaveWindowSettings)
                .DisposeWith(d);
        });
    }

    private IProjDashSettingsService SettingsService { get; }


    private WindowSettings LoadWindowSettings()
    {
        try
        {
            using var wndSettingsStream = File.OpenRead(SettingsService.WindowSettingsPath);
            return JsonSerializer.Deserialize(wndSettingsStream,
                SettingsSerializer.Default.WindowSettings);
        }
        catch (Exception ex)
        {
            this.Log().Warn(ex, "Error while loading window settings");
            return default;
        }
    }



    public ReactiveCommand<Unit, SettingsRoot> RefreshSettings { get; }
    public Interaction<ErrorDialogViewModel, Unit> ErrorDialog { get; } = new();

    public WindowSettings WindowSettings { get; set => this.RaiseAndSetIfChanged(ref field, value); }


    public ReactiveCommand<WindowSettings, Unit> SaveWindowSettings { get; }

    private async Task SaveWindowSettingsAsync(WindowSettings settings)
    {
        try
        {
            LogHost.Default.Debug($"Saving window settings {settings.Width}x{settings.Height}");
            var wndSettingsStream = File.Open(SettingsService.WindowSettingsPath, FileMode.Create);
            await using (wndSettingsStream.ConfigureAwait(false))
            {
                await JsonSerializer.SerializeAsync(wndSettingsStream, settings,
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
            this.Log().Info($"Trying to load {SettingsService.SettingsPath}");
            if (!File.Exists(SettingsService.SettingsPath))
            {
                await SaveSettings(new SettingsRoot() { Projects = [] }).ConfigureAwait(false);
            }
            var settingsStream = File.OpenRead(SettingsService.SettingsPath);
            await using (settingsStream.ConfigureAwait(false))
            {
                return await JsonSerializer.DeserializeAsync<SettingsRoot>(settingsStream,
                    SettingsSerializer.Default.SettingsRoot).ConfigureAwait(false) ?? SettingsRoot.Empty;
            }
        }
        catch (Exception ex)
        {
            this.Log().Warn(ex, "Error while loading settings");
            await ErrorDialog.Handle(new ErrorDialogViewModel
            {
                Exception = ex,
                MainMessage = $"Could not load settings from {SettingsService.SettingsPath}",
                PrimaryButtonText = "OK"
            });
            return SettingsRoot.Empty;
        }
    }


    private readonly ObservableAsPropertyHelper<SettingsRoot> _settings;
    public virtual SettingsRoot Settings => _settings.Value;
    public ReactiveCommand<ProjectViewModel, Unit> EditComplete { get; }

    public ReactiveCommand<ProjectViewModel, Unit> AddProject { get; }
    public ReactiveCommand<Project?, Unit> DeleteProject { get; }

    private async Task UpdateProjectAsync(ProjectViewModel newProject)
    {
        var newSettings = Settings with
        {
            Projects = Settings.Projects.Replace(newProject.OriginalProject, newProject.ToProject())
        };
        await SaveSettings(newSettings).ConfigureAwait(false);

        RefreshSettings.Execute().Subscribe();
    }

    private async Task SaveSettings(SettingsRoot newSettings)
    {
        var jsonStream = File.Open(SettingsService.SettingsPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        await using (jsonStream.ConfigureAwait(false))
        {
            await JsonSerializer.SerializeAsync(jsonStream, newSettings, SettingsSerializer.Default.SettingsRoot)
                .ConfigureAwait(false);
        }
    }

    private async Task AddProjectAsync(ProjectViewModel newProject)
    {
        var newSettings = Settings with { Projects = Settings.Projects.Add(newProject.ToProject()) };
        
        await SaveSettings(newSettings).ConfigureAwait(false);

        RefreshSettings.Execute().Subscribe();
    }

    private async Task DeleteProjectImpl(Project? project)
    {
        if (project is null)
            return;
        var newSettings = Settings with { Projects = Settings.Projects.Remove(project) };

        await SaveSettings(newSettings).ConfigureAwait(false);

        RefreshSettings.Execute().Subscribe();
    }

    public async Task SaveSchemaAsync()
    {
        var schemaFile = File.OpenWrite(SettingsService.SchemaPath);
        await using (schemaFile.ConfigureAwait(false))
        {
            Utf8JsonWriter writer = new(schemaFile);
            await using (writer.ConfigureAwait(false))
            {
                var schema = SettingsSerializer.Default.SettingsRoot.GetJsonSchemaAsNode();
                schema.WriteTo(writer);
            }
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
    Secondary
}