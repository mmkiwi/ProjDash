using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

using MMKiwi.ProjDash.ViewModel;

using Splat.Serilog;

using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;

using ReactiveUI;

using Serilog;

using Splat;

namespace MMKiwi.ProjDash.GUI;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        bool verbose = args.Contains("--verbose", StringComparer.CurrentCultureIgnoreCase);
        using var log = new LoggerConfiguration()
            .MinimumLevel.Is(verbose ? Serilog.Events.LogEventLevel.Verbose : Serilog.Events.LogEventLevel.Debug)
#if DEBUG
            .WriteTo.Debug()
            .WriteTo.Console()
#endif
            .WriteTo.File(MainWindowViewModel.LogPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        bool mutexCreated = false;
        using Mutex mutex = new(true, @"ProjDash.mutex", out mutexCreated);

        if (!mutexCreated)
        {
            Log.Error("Program already running. Exiting...");
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
            Log.CloseAndFlush();
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
        TaskScheduler.UnobservedTaskException += (sender, args) =>
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