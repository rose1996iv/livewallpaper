namespace WallpaperEngine.Core.Models;

public sealed class MonitorDescriptor
{
    public string DeviceName { get; set; } = string.Empty;

    public string FriendlyName { get; set; } = string.Empty;

    public int Left { get; set; }

    public int Top { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public bool IsPrimary { get; set; }

    public int Right => Left + Width;

    public int Bottom => Top + Height;
}
