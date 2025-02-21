// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

using MMKiwi.ProjDash.GUI.Converters;
using MMKiwi.ProjDash.Native;
using MMKiwi.ProjDash.ViewModel;
using MMKiwi.ProjDash.ViewModel.Model;

using ReactiveUI;

using Serilog;

using Splat;

namespace MMKiwi.ProjDash.GUI;

public class App : Application
{
    public App()
    {
        this.DataContext = this;
        IObservable<bool?> isVisible = _mainWindow.SelectMany(mw =>
            mw is null
                ? Observable.Return((bool?)null)
                : mw.GetObservable(Visual.IsVisibleProperty).Select(b => (bool?)b));
        ExitCommand = ReactiveCommand.Create(Exit);
        ToggleCommand = ReactiveCommand.Create(Toggle);
        ShowCommand = ReactiveCommand.Create(Show, isVisible.Select(iv => iv is false));
        HideCommand = ReactiveCommand.Create(Hide, isVisible.Select(iv => iv is true));
        LaunchMenuItem = ReactiveCommand.CreateFromTask<Uri>(LaunchMenuItemAsync);
    }

    public static new App? Current => Application.Current as App;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private readonly BehaviorSubject<NativeMenu?> _menu = new([]);
    public IObservable<NativeMenu?> Menu => _menu.AsObservable();

    private readonly BehaviorSubject<MainWindow?> _mainWindow = new(null);
    public MainWindow? MainWindow => _mainWindow.Value;

    public override void OnFrameworkInitializationCompleted()
    {
        Log.Verbose("App.OnFrameworkInitializationCompleted");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _mainWindow.BindTo(desktop, lt => lt.MainWindow);
            MainWindowViewModel mainWindowViewModel = new();
            _mainWindow.OnNext(new MainWindow { ViewModel = mainWindowViewModel });

            Locator.CurrentMutable.RegisterConstant(mainWindowViewModel);

            mainWindowViewModel.WhenAnyValue(vm => vm.Settings).Select(GetMenuItems)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_menu);
        }

        base.OnFrameworkInitializationCompleted();
        Log.Verbose("App.OnFrameworkInitializationCompleted");
    }

    private NativeMenu? GetMenuItems(SettingsRoot settings)
    {
        if (settings is not { Projects.Length: > 0 })
            return null;
        NativeMenu menu = [];
        foreach (var project in settings.Projects)
            menu.Add(ProjectToMenuItem(project));
        return menu;
    }

    private ReactiveCommand<Uri, Unit> LaunchMenuItem { get; }

    private async Task LaunchMenuItemAsync(Uri uri)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await TopLevel.GetTopLevel(MainWindow)!.Launcher.LaunchUriAsync(uri).ConfigureAwait(true);
        }).ConfigureAwait(true);
    }

    private NativeMenuItem ProjectToMenuItem(Project project)
    {
        LinkIconConverter conv = new();
        NativeMenu subMenu = [];
        foreach (ProjectLink link in project.Links)
        {
            Bitmap? bitmap = ImageToBitmap(conv.Convert(link.Icon, link.Color));
            NativeMenuItem submenuItem = new()
            {
                Header = link.Name, CommandParameter = link.Uri, Command = LaunchMenuItem, Icon = bitmap
            };
            subMenu.Add(submenuItem);
        }

        return new NativeMenuItem { Header = project.Title, Menu = subMenu };
    }

    private static Bitmap? ImageToBitmap(IImage? image)
    {
        if (image is null)
            return null;

        var pixelSize = new PixelSize((int)image.Size.Width, (int)image.Size.Height);
        using MemoryStream memoryStream = new();
        using (RenderTargetBitmap bitmap = new(pixelSize, new Vector(16, 16)))
        {
            using (DrawingContext ctx = bitmap.CreateDrawingContext())
            {
                image.Draw(ctx, new Rect(new Point(0, 0), image.Size), new Rect(new Point(0, 0), bitmap.Size));
            }

            bitmap.Save(memoryStream);
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        return new Bitmap(memoryStream);
    }

    public ReactiveCommand<Unit, Unit> HideCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowCommand { get; }
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleCommand { get; }

    private void Exit()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Log.Debug("Exiting application");
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
        });
    }

    private void Hide()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Log.Verbose("Hiding application");
            MainWindow?.Hide();
        });
    }

    internal void Show()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Log.Verbose("Showing application");
            MainWindow?.Show();
            MainWindow?.Activate();
        });
    }

    private void Toggle()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Log.Verbose($"Toggling application Visible: {MainWindow?.IsVisible}");
            if (MainWindow is null)
                return;

            bool isTop = false;

            try
            {
                var handle = MainWindow.TryGetPlatformHandle();
                if (handle is not null)
                {
                    isTop = NativeMethods.IsWindowOnTop(handle.Handle);
                }
            }
            catch (Exception e)
            {
                Log.Warning(e, "Error while trying to determine if window is on top");
            }

            switch (MainWindow.IsVisible, isTop)
            {
                case (true, true):
                    Log.Verbose("Hiding, window is visible and on top");
                    MainWindow.Hide();
                    break;
                case (true, false):
                    Log.Verbose("Activating, window is visible and obscured");
                    MainWindow.Activate();
                    break;
                case (false, _):
                    Log.Verbose("Activating, window is visible and obscured");
                    MainWindow.Show();
                    break;
            }
        });
    }
}