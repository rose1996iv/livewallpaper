namespace WallpaperEngine.Core.Models;

public sealed class WallpaperSession
{
    public Guid WallpaperId { get; set; }

    public string WallpaperName { get; set; } = string.Empty;

    public string SourcePath { get; set; } = string.Empty;

    public MonitorDescriptor TargetMonitor { get; set; } = new();

    public PlaybackProfile Playback { get; set; } = new();

    public bool SpanAcrossAllMonitors { get; set; }
}
