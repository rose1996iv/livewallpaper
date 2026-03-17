using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Core.Interfaces;

public interface ISettingsStore
{
    event EventHandler<AppSettings>? SettingsChanged;

    Task<AppSettings> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken);
}
