using Microsoft.Extensions.Logging;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Application.Services;

public sealed class WallpaperManager
{
    private static readonly string[] SupportedExtensions = [".mp4", ".webm"];
    private readonly IWallpaperRepository _wallpaperRepository;
    private readonly ILogger<WallpaperManager> _logger;

    public WallpaperManager(IWallpaperRepository wallpaperRepository, ILogger<WallpaperManager> logger)
    {
        _wallpaperRepository = wallpaperRepository;
        _logger = logger;
    }

    public Task<IReadOnlyList<WallpaperMetadata>> GetLibraryAsync(CancellationToken cancellationToken)
    {
        return _wallpaperRepository.GetAllAsync(cancellationToken);
    }

    public Task<WallpaperMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _wallpaperRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<WallpaperMetadata> ImportWallpaperAsync(string sourcePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException("A valid wallpaper file path is required.", nameof(sourcePath));
        }

        string extension = Path.GetExtension(sourcePath);
        if (!SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new NotSupportedException("Only MP4 and WEBM wallpapers are supported.");
        }

        WallpaperMetadata metadata = await _wallpaperRepository.ImportAsync(sourcePath, cancellationToken);
        _logger.LogInformation("Imported wallpaper {WallpaperName} from {SourcePath}", metadata.DisplayName, sourcePath);
        return metadata;
    }

    public async Task RemoveWallpaperAsync(Guid id, CancellationToken cancellationToken)
    {
        await _wallpaperRepository.RemoveAsync(id, cancellationToken);
        _logger.LogInformation("Removed wallpaper {WallpaperId}", id);
    }

    public string ResolveAbsolutePath(WallpaperMetadata metadata)
    {
        return _wallpaperRepository.ResolveAbsolutePath(metadata);
    }
}
