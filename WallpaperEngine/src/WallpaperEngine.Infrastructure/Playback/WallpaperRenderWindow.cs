using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using WallpaperEngine.Core.Models;
using WallpaperEngine.Infrastructure.Desktop;

namespace WallpaperEngine.Infrastructure.Playback;

internal sealed class WallpaperRenderWindow : Window
{
    private readonly WorkerWDesktopIntegrationService _desktopIntegrationService;
    private readonly ILogger _logger;
    private readonly MediaElement _mediaElement;

    public WallpaperRenderWindow(
        WallpaperSession session,
        WorkerWDesktopIntegrationService desktopIntegrationService,
        ILogger logger)
    {
        Session = session;
        _desktopIntegrationService = desktopIntegrationService;
        _logger = logger;

        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        ShowActivated = false;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Background = System.Windows.Media.Brushes.Black;
        Left = session.TargetMonitor.Left;
        Top = session.TargetMonitor.Top;
        Width = session.TargetMonitor.Width;
        Height = session.TargetMonitor.Height;

        _mediaElement = new MediaElement
        {
            LoadedBehavior = MediaState.Manual,
            UnloadedBehavior = MediaState.Stop,
            Stretch = MapStretch(session.Playback),
            Volume = session.Playback.IsMuted ? 0 : Math.Clamp(session.Playback.Volume, 0, 1),
            SpeedRatio = Math.Clamp(session.Playback.PlaybackSpeed, 0.25, 4.0),
            ScrubbingEnabled = false
        };

        _mediaElement.MediaEnded += (_, _) =>
        {
            _mediaElement.Position = TimeSpan.Zero;
            _mediaElement.Play();
        };

        _mediaElement.MediaFailed += (_, args) =>
        {
            _logger.LogError(args.ErrorException, "Failed to play wallpaper {WallpaperName}", Session.WallpaperName);
        };

        Content = _mediaElement;
        SourceInitialized += OnSourceInitialized;
        Closed += (_, _) => _mediaElement.Stop();
    }

    public WallpaperSession Session { get; }

    public void Start()
    {
        _mediaElement.Source = new Uri(Session.SourcePath, UriKind.Absolute);
        _mediaElement.Position = TimeSpan.Zero;
        _mediaElement.Play();
    }

    public void Pause()
    {
        _mediaElement.Pause();
    }

    public void Resume()
    {
        _mediaElement.Play();
    }

    public void StopPlayback()
    {
        _mediaElement.Stop();
    }

    public void ReattachToDesktop()
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        if (handle != IntPtr.Zero)
        {
            _desktopIntegrationService.AttachWindow(handle);
        }
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        _desktopIntegrationService.HideFromAltTab(handle);
        _desktopIntegrationService.AttachWindow(handle);
    }

    private static Stretch MapStretch(PlaybackProfile playback)
    {
        if (!playback.MaintainAspectRatio)
        {
            return Stretch.Fill;
        }

        return playback.StretchMode switch
        {
            WallpaperStretchMode.Fit => Stretch.Uniform,
            _ => Stretch.UniformToFill
        };
    }
}
