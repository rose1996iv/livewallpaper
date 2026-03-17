using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Core.Interfaces;

public interface IVideoPlaybackEngine
{
    Task ApplyAsync(IReadOnlyList<WallpaperSession> sessions, CancellationToken cancellationToken);

    Task PauseAsync(CancellationToken cancellationToken);

    Task ResumeAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);

    Task RefreshDesktopBindingAsync(CancellationToken cancellationToken);

    PlaybackRuntimeState GetState();
}
