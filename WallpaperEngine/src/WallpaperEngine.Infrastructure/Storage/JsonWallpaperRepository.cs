using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;
using WallpaperEngine.Infrastructure.Hosting;

namespace WallpaperEngine.Infrastructure.Storage;

public sealed class JsonWallpaperRepository : IWallpaperRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly AppPaths _paths;
    private readonly ILogger<JsonWallpaperRepository> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonWallpaperRepository(AppPaths paths, ILogger<JsonWallpaperRepository> logger)
    {
        _paths = paths;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WallpaperMetadata>> GetAllAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await LoadLibraryAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<WallpaperMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        IReadOnlyList<WallpaperMetadata> wallpapers = await GetAllAsync(cancellationToken);
        return wallpapers.FirstOrDefault(wallpaper => wallpaper.Id == id);
    }

    public async Task<WallpaperMetadata> ImportAsync(string sourcePath, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_paths.WallpaperDirectory);

            List<WallpaperMetadata> wallpapers = await LoadLibraryAsync(cancellationToken);
            string extension = Path.GetExtension(sourcePath);
            string safeFileName = $"{SanitizeFileName(Path.GetFileNameWithoutExtension(sourcePath))}-{Guid.NewGuid():N}{extension}";
            string destinationPath = Path.Combine(_paths.WallpaperDirectory, safeFileName);

            File.Copy(sourcePath, destinationPath, overwrite: false);

            WallpaperMetadata metadata = new()
            {
                DisplayName = Path.GetFileNameWithoutExtension(sourcePath),
                FileName = safeFileName,
                RelativePath = Path.Combine("assets", "wallpapers", safeFileName),
                MediaType = MapMediaType(extension),
                AddedUtc = DateTimeOffset.UtcNow
            };

            wallpapers.Add(metadata);
            await SaveLibraryAsync(wallpapers, cancellationToken);
            return metadata;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            List<WallpaperMetadata> wallpapers = await LoadLibraryAsync(cancellationToken);
            WallpaperMetadata? metadata = wallpapers.FirstOrDefault(item => item.Id == id);
            if (metadata is null)
            {
                return;
            }

            wallpapers.Remove(metadata);
            await SaveLibraryAsync(wallpapers, cancellationToken);

            string absolutePath = ResolveAbsolutePath(metadata);
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public string ResolveAbsolutePath(WallpaperMetadata wallpaper)
    {
        return Path.Combine(_paths.BasePath, wallpaper.RelativePath);
    }

    private async Task<List<WallpaperMetadata>> LoadLibraryAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_paths.ConfigDirectory);

        if (!File.Exists(_paths.WallpaperLibraryFilePath))
        {
            await SaveLibraryAsync([], cancellationToken);
            return [];
        }

        string content = await File.ReadAllTextAsync(_paths.WallpaperLibraryFilePath, cancellationToken);
        List<WallpaperMetadata>? wallpapers = JsonSerializer.Deserialize<List<WallpaperMetadata>>(content, SerializerOptions);
        return wallpapers ?? [];
    }

    private async Task SaveLibraryAsync(List<WallpaperMetadata> wallpapers, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(
            wallpapers.OrderBy(wallpaper => wallpaper.DisplayName, StringComparer.OrdinalIgnoreCase),
            SerializerOptions);

        await File.WriteAllTextAsync(_paths.WallpaperLibraryFilePath, json, cancellationToken);
        _logger.LogInformation("Wallpaper library saved with {Count} items.", wallpapers.Count);
    }

    private static WallpaperMediaType MapMediaType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".mp4" => WallpaperMediaType.Mp4,
            ".webm" => WallpaperMediaType.WebM,
            _ => WallpaperMediaType.Unknown
        };
    }

    private static string SanitizeFileName(string value)
    {
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidCharacter, '-');
        }

        return value;
    }
}
