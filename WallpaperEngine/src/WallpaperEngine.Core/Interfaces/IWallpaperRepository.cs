using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Core.Interfaces;

public interface IWallpaperRepository
{
    Task<IReadOnlyList<WallpaperMetadata>> GetAllAsync(CancellationToken cancellationToken);

    Task<WallpaperMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<WallpaperMetadata> ImportAsync(string sourcePath, CancellationToken cancellationToken);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken);

    string ResolveAbsolutePath(WallpaperMetadata wallpaper);
}
