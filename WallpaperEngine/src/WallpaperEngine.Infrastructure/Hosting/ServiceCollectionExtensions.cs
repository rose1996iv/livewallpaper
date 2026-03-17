using System.IO;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Infrastructure.Desktop;
using WallpaperEngine.Infrastructure.Fullscreen;
using WallpaperEngine.Infrastructure.Hosting;
using WallpaperEngine.Infrastructure.Monitoring;
using WallpaperEngine.Infrastructure.Playback;
using WallpaperEngine.Infrastructure.Startup;
using WallpaperEngine.Infrastructure.Storage;
using WallpaperEngine.Infrastructure.SystemTray;

namespace WallpaperEngine.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWallpaperEngineInfrastructure(
        this IServiceCollection services,
        string basePath,
        Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(basePath);
        ArgumentNullException.ThrowIfNull(dispatcher);

        AppPaths paths = new(basePath);
        Directory.CreateDirectory(paths.ConfigDirectory);
        Directory.CreateDirectory(paths.WallpaperDirectory);
        Directory.CreateDirectory(paths.LogsDirectory);

        services.AddSingleton(paths);
        services.AddSingleton(dispatcher);
        services.AddSingleton<WorkerWDesktopIntegrationService>();
        services.AddSingleton<ISettingsStore, JsonSettingsStore>();
        services.AddSingleton<IWallpaperRepository, JsonWallpaperRepository>();
        services.AddSingleton<IMonitorProvider, WindowsMonitorProvider>();
        services.AddSingleton<IFullscreenStateProvider, WindowsFullscreenStateProvider>();
        services.AddSingleton<IVideoPlaybackEngine, MediaElementVideoPlaybackEngine>();
        services.AddSingleton<ISystemTrayService, NotifyIconSystemTrayService>();
        services.AddSingleton<IStartupManager, RegistryStartupManager>();

        return services;
    }
}
