namespace WallpaperEngine.Core.Models;

public sealed class PlaybackProfile
{
    public double Volume { get; set; } = 0;

    public double PlaybackSpeed { get; set; } = 1.0;

    public bool IsMuted { get; set; } = true;

    public bool UseHardwareAcceleration { get; set; } = true;

    public WallpaperStretchMode StretchMode { get; set; } = WallpaperStretchMode.Fill;

    public bool MaintainAspectRatio { get; set; } = true;

    public PlaybackProfile DeepCopy()
    {
        return new PlaybackProfile
        {
            Volume = Volume,
            PlaybackSpeed = PlaybackSpeed,
            IsMuted = IsMuted,
            UseHardwareAcceleration = UseHardwareAcceleration,
            StretchMode = StretchMode,
            MaintainAspectRatio = MaintainAspectRatio
        };
    }
}
