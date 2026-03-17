namespace WallpaperEngine.Core.Interfaces;

public interface IStartupManager
{
    Task<bool> IsEnabledAsync(CancellationToken cancellationToken);

    Task SetEnabledAsync(bool enabled, string executablePath, CancellationToken cancellationToken);
}
