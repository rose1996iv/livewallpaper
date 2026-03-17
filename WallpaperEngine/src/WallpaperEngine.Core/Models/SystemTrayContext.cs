namespace WallpaperEngine.Core.Models;

public sealed class SystemTrayContext
{
    public Func<Task>? OpenSettingsAsync { get; set; }

    public Func<Task>? OpenLibraryAsync { get; set; }

    public Func<Task>? TogglePlaybackAsync { get; set; }

    public Func<Task>? ExitAsync { get; set; }
}
