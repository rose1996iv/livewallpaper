using Microsoft.Extensions.Logging;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Application.Services;

public sealed class PlaybackController
{
    private readonly MonitorManager _monitorManager;
    private readonly WallpaperManager _wallpaperManager;
    private readonly SettingsService _settingsService;
    private readonly IVideoPlaybackEngine _videoPlaybackEngine;
    private readonly ILogger<PlaybackController> _logger;
    private readonly HashSet<PauseReason> _pauseReasons = [];

    public PlaybackController(
        MonitorManager monitorManager,
        WallpaperManager wallpaperManager,
        SettingsService settingsService,
        IVideoPlaybackEngine videoPlaybackEngine,
        ILogger<PlaybackController> logger)
    {
        _monitorManager = monitorManager;
        _wallpaperManager = wallpaperManager;
        _settingsService = settingsService;
        _videoPlaybackEngine = videoPlaybackEngine;
        _logger = logger;
    }

    public event EventHandler<PlaybackRuntimeState>? StateChanged;

    public async Task ApplyCurrentWallpaperAsync(CancellationToken cancellationToken)
    {
        AppSettings settings = await _settingsService.GetCurrentAsync(cancellationToken);
        IReadOnlyList<WallpaperSession> sessions = await BuildSessionsAsync(settings, cancellationToken);

        if (sessions.Count == 0)
        {
            _logger.LogWarning("No wallpaper sessions could be built from current settings.");
            await _videoPlaybackEngine.StopAsync(cancellationToken);
            PublishState();
            return;
        }

        await _videoPlaybackEngine.ApplyAsync(sessions, cancellationToken);

        if (settings.StartPaused || _pauseReasons.Count > 0)
        {
            await _videoPlaybackEngine.PauseAsync(cancellationToken);
        }
        else
        {
            await _videoPlaybackEngine.ResumeAsync(cancellationToken);
        }

        _logger.LogInformation("Applied {SessionCount} wallpaper sessions.", sessions.Count);
        PublishState();
    }

    public async Task SetPauseAsync(PauseReason reason, bool paused, CancellationToken cancellationToken)
    {
        bool changed = paused ? _pauseReasons.Add(reason) : _pauseReasons.Remove(reason);
        if (!changed)
        {
            return;
        }

        if (_pauseReasons.Count > 0)
        {
            await _videoPlaybackEngine.PauseAsync(cancellationToken);
            _logger.LogInformation("Playback paused due to {Reason}.", reason);
        }
        else
        {
            await _videoPlaybackEngine.ResumeAsync(cancellationToken);
            _logger.LogInformation("Playback resumed after {Reason}.", reason);
        }

        PublishState();
    }

    public Task ToggleUserPauseAsync(CancellationToken cancellationToken)
    {
        bool shouldPause = !_pauseReasons.Contains(PauseReason.User);
        return SetPauseAsync(PauseReason.User, shouldPause, cancellationToken);
    }

    public Task RefreshDesktopAttachmentAsync(CancellationToken cancellationToken)
    {
        return _videoPlaybackEngine.RefreshDesktopBindingAsync(cancellationToken);
    }

    public PlaybackRuntimeState GetCurrentState()
    {
        PlaybackRuntimeState state = _videoPlaybackEngine.GetState();
        state.IsPaused = _pauseReasons.Count > 0 || state.IsPaused;
        return state;
    }

    public async Task<IReadOnlyList<WallpaperSession>> BuildSessionsAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        IReadOnlyList<MonitorDescriptor> monitors = await _monitorManager.GetMonitorsAsync(cancellationToken);
        IReadOnlyList<WallpaperMetadata> wallpapers = await _wallpaperManager.GetLibraryAsync(cancellationToken);
        Dictionary<Guid, WallpaperMetadata> lookup = wallpapers.ToDictionary(wallpaper => wallpaper.Id);

        if (monitors.Count == 0 || lookup.Count == 0)
        {
            return Array.Empty<WallpaperSession>();
        }

        if (settings.LayoutMode == WallpaperLayoutMode.Span)
        {
            Guid? spanWallpaperId = settings.SpanWallpaperId ?? settings.DefaultWallpaperId ?? lookup.Keys.FirstOrDefault();
            if (spanWallpaperId is null || !lookup.TryGetValue(spanWallpaperId.Value, out WallpaperMetadata? wallpaper))
            {
                return Array.Empty<WallpaperSession>();
            }

            MonitorDescriptor virtualMonitor = BuildVirtualMonitor(monitors);
            return [CreateSession(wallpaper, virtualMonitor, settings.Playback, true)];
        }

        List<WallpaperSession> sessions = [];
        foreach (MonitorDescriptor monitor in monitors)
        {
            Guid? wallpaperId = ResolveWallpaperIdForMonitor(settings, monitor, lookup.Keys);
            if (wallpaperId is null || !lookup.TryGetValue(wallpaperId.Value, out WallpaperMetadata? wallpaper))
            {
                continue;
            }

            sessions.Add(CreateSession(wallpaper, monitor, settings.Playback, false));
        }

        return sessions;
    }

    private Guid? ResolveWallpaperIdForMonitor(
        AppSettings settings,
        MonitorDescriptor monitor,
        IEnumerable<Guid> availableWallpaperIds)
    {
        Guid? assignedWallpaperId = settings.MonitorAssignments
            .FirstOrDefault(assignment => string.Equals(assignment.MonitorDeviceName, monitor.DeviceName, StringComparison.OrdinalIgnoreCase))
            ?.WallpaperId;

        if (assignedWallpaperId.HasValue)
        {
            return assignedWallpaperId.Value;
        }

        if (settings.DefaultWallpaperId.HasValue)
        {
            return settings.DefaultWallpaperId.Value;
        }

        return availableWallpaperIds.FirstOrDefault();
    }

    private WallpaperSession CreateSession(
        WallpaperMetadata wallpaper,
        MonitorDescriptor monitor,
        PlaybackProfile playbackProfile,
        bool spanAcrossAllMonitors)
    {
        return new WallpaperSession
        {
            WallpaperId = wallpaper.Id,
            WallpaperName = wallpaper.DisplayName,
            SourcePath = _wallpaperManager.ResolveAbsolutePath(wallpaper),
            TargetMonitor = monitor,
            Playback = playbackProfile.DeepCopy(),
            SpanAcrossAllMonitors = spanAcrossAllMonitors
        };
    }

    private static MonitorDescriptor BuildVirtualMonitor(IReadOnlyList<MonitorDescriptor> monitors)
    {
        int left = monitors.Min(monitor => monitor.Left);
        int top = monitors.Min(monitor => monitor.Top);
        int right = monitors.Max(monitor => monitor.Right);
        int bottom = monitors.Max(monitor => monitor.Bottom);

        return new MonitorDescriptor
        {
            DeviceName = "SPAN",
            FriendlyName = "All Monitors",
            Left = left,
            Top = top,
            Width = right - left,
            Height = bottom - top,
            IsPrimary = true
        };
    }

    private void PublishState()
    {
        PlaybackRuntimeState state = GetCurrentState();
        StateChanged?.Invoke(this, state);
    }
}
