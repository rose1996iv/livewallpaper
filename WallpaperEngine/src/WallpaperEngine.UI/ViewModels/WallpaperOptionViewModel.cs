using WallpaperEngine.Core.Models;

namespace WallpaperEngine.UI.ViewModels;

public sealed class WallpaperOptionViewModel
{
    public Guid Id { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public string RelativePath { get; init; } = string.Empty;

    public WallpaperMediaType MediaType { get; init; }

    public static WallpaperOptionViewModel FromMetadata(WallpaperMetadata metadata)
    {
        return new WallpaperOptionViewModel
        {
            Id = metadata.Id,
            DisplayName = metadata.DisplayName,
            RelativePath = metadata.RelativePath,
            MediaType = metadata.MediaType
        };
    }
}
