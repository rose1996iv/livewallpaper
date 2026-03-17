namespace WallpaperEngine.Core.Models;

public sealed class PlaybackRuntimeState
{
    public bool IsPaused { get; set; }

    public bool IsRendering { get; set; }

    public IReadOnlyList<WallpaperSession> ActiveSessions { get; set; } = Array.Empty<WallpaperSession>();
}
