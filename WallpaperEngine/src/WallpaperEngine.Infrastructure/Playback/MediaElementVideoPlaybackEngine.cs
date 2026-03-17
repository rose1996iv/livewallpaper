using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;
using WallpaperEngine.Infrastructure.Desktop;

namespace WallpaperEngine.Infrastructure.Playback;

public sealed class MediaElementVideoPlaybackEngine : IVideoPlaybackEngine
{
    private readonly Dispatcher _dispatcher;
    private readonly WorkerWDesktopIntegrationService _desktopIntegrationService;
    private readonly ILogger<MediaElementVideoPlaybackEngine> _logger;
    private readonly List<WallpaperRenderWindow> _windows = [];
    private PlaybackRuntimeState _state = new();

    public MediaElementVideoPlaybackEngine(
        Dispatcher dispatcher,
        WorkerWDesktopIntegrationService desktopIntegrationService,
        ILogger<MediaElementVideoPlaybackEngine> logger)
    {
        _dispatcher = dispatcher;
        _desktopIntegrationService = desktopIntegrationService;
        _logger = logger;
    }

    public async Task ApplyAsync(IReadOnlyList<WallpaperSession> sessions, CancellationToken cancellationToken)
    {
        await _dispatcher.InvokeAsync(() =>
        {
            StopInternal();

            bool hardwareAccelerationEnabled = sessions.All(session => session.Playback.UseHardwareAcceleration);
            RenderOptions.ProcessRenderMode = hardwareAccelerationEnabled
                ? RenderMode.Default
                : RenderMode.SoftwareOnly;

            foreach (WallpaperSession session in sessions)
            {
                WallpaperRenderWindow window = new(session, _desktopIntegrationService, _logger);
                _windows.Add(window);
                window.Show();
                window.Start();
            }

            _state = new PlaybackRuntimeState
            {
                IsPaused = false,
                IsRendering = _windows.Count > 0,
                ActiveSessions = sessions.ToArray()
            };
        }, DispatcherPriority.Normal, cancellationToken);
    }

    public async Task PauseAsync(CancellationToken cancellationToken)
    {
        await _dispatcher.InvokeAsync(() =>
        {
            foreach (WallpaperRenderWindow window in _windows)
            {
                window.Pause();
            }

            _state.IsPaused = true;
        }, DispatcherPriority.Normal, cancellationToken);
    }

    public async Task ResumeAsync(CancellationToken cancellationToken)
    {
        await _dispatcher.InvokeAsync(() =>
        {
            foreach (WallpaperRenderWindow window in _windows)
            {
                window.Resume();
            }

            _state.IsPaused = false;
        }, DispatcherPriority.Normal, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _dispatcher.InvokeAsync(StopInternal, DispatcherPriority.Normal, cancellationToken);
    }

    public async Task RefreshDesktopBindingAsync(CancellationToken cancellationToken)
    {
        await _dispatcher.InvokeAsync(() =>
        {
            foreach (WallpaperRenderWindow window in _windows.Where(window => window.IsLoaded))
            {
                window.ReattachToDesktop();
            }
        }, DispatcherPriority.Background, cancellationToken);
    }

    public PlaybackRuntimeState GetState()
    {
        return new PlaybackRuntimeState
        {
            IsPaused = _state.IsPaused,
            IsRendering = _state.IsRendering,
            ActiveSessions = _state.ActiveSessions.ToArray()
        };
    }

    private void StopInternal()
    {
        foreach (WallpaperRenderWindow window in _windows.ToArray())
        {
            window.StopPlayback();
            window.Close();
        }

        _windows.Clear();
        _state = new PlaybackRuntimeState
        {
            IsPaused = false,
            IsRendering = false,
            ActiveSessions = Array.Empty<WallpaperSession>()
        };
    }
}
