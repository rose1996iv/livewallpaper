using System.IO;

namespace WallpaperEngine.Infrastructure.Hosting;

public sealed class AppPaths
{
    public AppPaths(string basePath)
    {
        BasePath = basePath;
        ConfigDirectory = Path.Combine(basePath, "config");
        AssetsDirectory = Path.Combine(basePath, "assets");
        WallpaperDirectory = Path.Combine(AssetsDirectory, "wallpapers");
        LogsDirectory = Path.Combine(basePath, "logs");
        SettingsFilePath = Path.Combine(ConfigDirectory, "settings.json");
        WallpaperLibraryFilePath = Path.Combine(ConfigDirectory, "wallpaper-library.json");
        LogFilePath = Path.Combine(LogsDirectory, "app.log");
    }

    public string BasePath { get; }

    public string ConfigDirectory { get; }

    public string AssetsDirectory { get; }

    public string WallpaperDirectory { get; }

    public string LogsDirectory { get; }

    public string SettingsFilePath { get; }

    public string WallpaperLibraryFilePath { get; }

    public string LogFilePath { get; }
}
