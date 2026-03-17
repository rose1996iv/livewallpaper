using Microsoft.Extensions.Logging.Abstractions;
using WallpaperEngine.Application.Services;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Tests;

public sealed class WallpaperManagerTests
{
    [Fact]
    public async Task ImportWallpaperAsync_RejectsUnsupportedExtensions()
    {
        WallpaperManager manager = new(new FakeWallpaperRepository(), NullLogger<WallpaperManager>.Instance);

        await Assert.ThrowsAsync<NotSupportedException>(() => manager.ImportWallpaperAsync("demo.avi", CancellationToken.None));
    }

    private sealed class FakeWallpaperRepository : IWallpaperRepository
    {
        public Task<IReadOnlyList<WallpaperMetadata>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WallpaperMetadata>>(Array.Empty<WallpaperMetadata>());
        }

        public Task<WallpaperMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult<WallpaperMetadata?>(null);
        }

        public Task<WallpaperMetadata> ImportAsync(string sourcePath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string ResolveAbsolutePath(WallpaperMetadata wallpaper)
        {
            return wallpaper.RelativePath;
        }
    }
}
