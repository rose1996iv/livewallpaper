using System.Text.Json.Serialization;

namespace WallpaperEngine.Core.Models;

public sealed class WallpaperMetadata
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string DisplayName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public WallpaperMediaType MediaType { get; set; }

    public DateTimeOffset AddedUtc { get; set; } = DateTimeOffset.UtcNow;

    public string FileName { get; set; } = string.Empty;

    [JsonIgnore]
    public string Description => $"{DisplayName} ({MediaType})";
}
