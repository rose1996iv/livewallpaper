namespace WallpaperEngine.Core.Models;

public sealed class SystemTrayState
{
    public string TooltipText { get; set; } = "Wallpaper Engine";

    public bool IsPaused { get; set; }

    public string ActiveWallpaperSummary { get; set; } = "No wallpaper loaded";
}
