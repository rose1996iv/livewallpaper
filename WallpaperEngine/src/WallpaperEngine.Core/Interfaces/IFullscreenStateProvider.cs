using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Core.Interfaces;

public interface IFullscreenStateProvider
{
    Task<FullscreenInfo> GetFullscreenInfoAsync(CancellationToken cancellationToken);
}
