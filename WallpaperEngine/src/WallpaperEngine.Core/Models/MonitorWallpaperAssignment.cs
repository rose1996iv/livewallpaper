namespace WallpaperEngine.Core.Models;

public sealed class MonitorWallpaperAssignment
{
    public string MonitorDeviceName { get; set; } = string.Empty;

    public Guid? WallpaperId { get; set; }

    public MonitorWallpaperAssignment DeepCopy()
    {
        return new MonitorWallpaperAssignment
        {
            MonitorDeviceName = MonitorDeviceName,
            WallpaperId = WallpaperId
        };
    }
}
