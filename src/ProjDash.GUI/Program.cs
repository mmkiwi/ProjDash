// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.ViewModel;

using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;

using ReactiveUI;

using Serilog;
using Serilog.Events;

using Splat;
using Splat.Serilog;

namespace MMKiwi.ProjDash.GUI;

static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args)
    {
        bool verbose = args.Contains("--verbose", StringComparer.CurrentCultureIgnoreCase);
        var log = new LoggerConfiguration()
            .MinimumLevel.Is(verbose ? LogEventLevel.Verbose : LogEventLevel.Debug)
#if DEBUG
            .WriteTo.Debug()
            .WriteTo.Console()
#endif
            .WriteTo.File(MainWindowViewModel.LogPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();
        await using (log.ConfigureAwait(false))
        {
            using Mutex mutex = new(true, "ProjDash.mutex", out bool mutexCreated);

            if (!mutexCreated)
            {
                Log.Error("Program already running. Exiting...");
                await WindowPipe.SendMessageAsync().ConfigureAwait(false);
                return;
            }


            Log.Logger = log;
            Locator.CurrentMutable.UseSerilogFullLogger();

            try
            {
                Log.Information("Startup");

                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly");
            }
            finally
            {
                await Log.CloseAndFlushAsync().ConfigureAwait(false);
            }
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        if (Design.IsDesignMode)
        {
            using var log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
#if DEBUG
                .WriteTo.Debug()
                .WriteTo.Console()
#endif
                .WriteTo.File(MainWindowViewModel.LogPath + ".design.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Logger = log;
            Locator.CurrentMutable.UseSerilogFullLogger();
        }

        LogHost.Default.Debug("Building Avalonia");

        IconProvider.Current.Register<MaterialDesignIconProvider>();

        RxApp.DefaultExceptionHandler = new HandleErrors();
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Log.Logger.Fatal(args.Exception, "Unobserved task exception");
        };

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
}