namespace WallpaperEngine.Core.Models;

public sealed class FullscreenInfo
{
    public bool IsFullscreen { get; set; }

    public string ProcessName { get; set; } = string.Empty;

    public string WindowTitle { get; set; } = string.Empty;
}
