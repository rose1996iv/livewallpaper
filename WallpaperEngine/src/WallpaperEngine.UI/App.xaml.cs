using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WallpaperEngine.Application.Services;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;
using WallpaperEngine.Infrastructure;
using WallpaperEngine.Infrastructure.Logging;
using WallpaperEngine.UI.ViewModels;
using WallpaperEngine.UI.Views;

namespace WallpaperEngine.UI;

public partial class App : System.Windows.Application
{
    private IHost? _host;
    private MainWindow? _settingsWindow;
    private WallpaperLibraryWindow? _libraryWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddWallpaperEngineFileLogger();
            })
            .ConfigureServices((_, services) =>
            {
                services.AddWallpaperEngineInfrastructure(AppContext.BaseDirectory, Dispatcher);
                services.AddSingleton<MonitorManager>();
                services.AddSingleton<WallpaperManager>();
                services.AddSingleton<SettingsService>();
                services.AddSingleton<PlaybackController>();
                services.AddHostedService<FullscreenDetector>();
                services.AddHostedService<DesktopRecoveryService>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<WallpaperLibraryViewModel>();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<WallpaperLibraryWindow>();
            })
            .Build();

        await _host.StartAsync();

        _settingsWindow = _host.Services.GetRequiredService<MainWindow>();
        _libraryWindow = _host.Services.GetRequiredService<WallpaperLibraryWindow>();

        SettingsViewModel settingsViewModel = _host.Services.GetRequiredService<SettingsViewModel>();
        WallpaperLibraryViewModel libraryViewModel = _host.Services.GetRequiredService<WallpaperLibraryViewModel>();
        PlaybackController playbackController = _host.Services.GetRequiredService<PlaybackController>();
        ISystemTrayService trayService = _host.Services.GetRequiredService<ISystemTrayService>();

        settingsViewModel.OpenLibraryRequested += OnOpenLibraryRequested;
        settingsViewModel.LibraryRefreshRequested += OnLibraryRefreshRequested;
        settingsViewModel.SettingsApplied += OnSettingsApplied;
        libraryViewModel.LibraryChanged += OnLibraryChanged;
        playbackController.StateChanged += (_, state) => UpdateTray(trayService, state);

        await settingsViewModel.InitializeAsync();
        await libraryViewModel.InitializeAsync();
        await playbackController.ApplyCurrentWallpaperAsync(CancellationToken.None);

        trayService.Initialize(new SystemTrayContext
        {
            OpenSettingsAsync = ShowSettingsWindowAsync,
            OpenLibraryAsync = ShowLibraryWindowAsync,
            TogglePlaybackAsync = () => TogglePlaybackAsync(playbackController, trayService),
            ExitAsync = ExitApplicationAsync
        });

        PlaybackRuntimeState state = playbackController.GetCurrentState();
        UpdateTray(trayService, state);

        if (ShouldShowSettingsWindow(e.Args, state))
        {
            _settingsWindow.Show();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_libraryWindow is not null)
        {
            _libraryWindow.AllowClose = true;
        }

        if (_settingsWindow is not null)
        {
            _settingsWindow.AllowClose = true;
        }

        if (_host is not null)
        {
            _host.Services.GetService<ISystemTrayService>()?.Dispose();
            await _host.StopAsync(TimeSpan.FromSeconds(3));
            _host.Dispose();
        }

        base.OnExit(e);
    }

    private async Task ShowSettingsWindowAsync()
    {
        await Dispatcher.InvokeAsync(() =>
        {
            _settingsWindow?.Show();
            if (_settingsWindow?.WindowState == WindowState.Minimized)
            {
                _settingsWindow.WindowState = WindowState.Normal;
            }

            _settingsWindow?.Activate();
        });
    }

    private async Task ShowLibraryWindowAsync()
    {
        await Dispatcher.InvokeAsync(() =>
        {
            _libraryWindow?.Show();
            if (_libraryWindow?.WindowState == WindowState.Minimized)
            {
                _libraryWindow.WindowState = WindowState.Normal;
            }

            _libraryWindow?.Activate();
        });
    }

    private async Task TogglePlaybackAsync(PlaybackController playbackController, ISystemTrayService trayService)
    {
        await playbackController.ToggleUserPauseAsync(CancellationToken.None);
        UpdateTray(trayService, playbackController.GetCurrentState());
    }

    private async Task ExitApplicationAsync()
    {
        await Dispatcher.InvokeAsync(Shutdown);
    }

    private async void OnOpenLibraryRequested(object? sender, EventArgs e)
    {
        await ShowLibraryWindowAsync();
    }

    private async void OnLibraryRefreshRequested(object? sender, EventArgs e)
    {
        if (_host is null)
        {
            return;
        }

        WallpaperLibraryViewModel libraryViewModel = _host.Services.GetRequiredService<WallpaperLibraryViewModel>();
        await libraryViewModel.InitializeAsync();
    }

    private async void OnLibraryChanged(object? sender, EventArgs e)
    {
        if (_host is null)
        {
            return;
        }

        SettingsViewModel settingsViewModel = _host.Services.GetRequiredService<SettingsViewModel>();
        await settingsViewModel.InitializeAsync();
    }

    private async void OnSettingsApplied(object? sender, EventArgs e)
    {
        await Dispatcher.InvokeAsync(() =>
        {
            _libraryWindow?.Hide();
            _settingsWindow?.Hide();
        });
    }

    private static bool ShouldShowSettingsWindow(string[] startupArguments, PlaybackRuntimeState state)
    {
        if (startupArguments.Any(argument =>
                string.Equals(argument, "--settings", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(argument, "/settings", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return state.ActiveSessions.Count == 0;
    }

    private static void UpdateTray(ISystemTrayService trayService, PlaybackRuntimeState state)
    {
        string summary = state.ActiveSessions.Count == 0
            ? "No wallpaper loaded"
            : string.Join(", ", state.ActiveSessions.Select(session => session.WallpaperName).Distinct());

        trayService.UpdateState(new SystemTrayState
        {
            TooltipText = "Wallpaper Engine",
            IsPaused = state.IsPaused,
            ActiveWallpaperSummary = summary
        });
    }
}
