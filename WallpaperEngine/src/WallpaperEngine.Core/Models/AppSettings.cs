namespace WallpaperEngine.Core.Models;

public sealed class AppSettings
{
    public WallpaperLayoutMode LayoutMode { get; set; } = WallpaperLayoutMode.PerMonitor;

    public Guid? DefaultWallpaperId { get; set; }

    public Guid? SpanWallpaperId { get; set; }

    public bool PauseOnFullscreen { get; set; } = true;

    public bool StartWithWindows { get; set; }

    public bool StartPaused { get; set; }

    public PlaybackProfile Playback { get; set; } = new();

    public List<MonitorWallpaperAssignment> MonitorAssignments { get; set; } = [];

    public AppSettings DeepCopy()
    {
        return new AppSettings
        {
            LayoutMode = LayoutMode,
            DefaultWallpaperId = DefaultWallpaperId,
            SpanWallpaperId = SpanWallpaperId,
            PauseOnFullscreen = PauseOnFullscreen,
            StartWithWindows = StartWithWindows,
            StartPaused = StartPaused,
            Playback = Playback.DeepCopy(),
            MonitorAssignments = MonitorAssignments.ConvertAll(assignment => assignment.DeepCopy())
        };
    }
}
