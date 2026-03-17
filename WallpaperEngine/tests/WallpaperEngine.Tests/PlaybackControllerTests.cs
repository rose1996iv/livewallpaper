using Microsoft.Extensions.Logging.Abstractions;
using WallpaperEngine.Application.Services;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Tests;

public sealed class PlaybackControllerTests
{
    [Fact]
    public async Task BuildSessionsAsync_ReturnsPerMonitorSessions_WhenPerMonitorModeIsEnabled()
    {
        FakeWallpaperRepository wallpaperRepository = new();
        WallpaperMetadata wallpaper = wallpaperRepository.AddWallpaper("Primary");
        FakeMonitorProvider monitorProvider = new([
            new MonitorDescriptor { DeviceName = "DISPLAY1", FriendlyName = "Display 1", Left = 0, Top = 0, Width = 1920, Height = 1080, IsPrimary = true },
            new MonitorDescriptor { DeviceName = "DISPLAY2", FriendlyName = "Display 2", Left = 1920, Top = 0, Width = 1920, Height = 1080, IsPrimary = false }
        ]);

        PlaybackController controller = CreatePlaybackController(wallpaperRepository, monitorProvider, new AppSettings
        {
            LayoutMode = WallpaperLayoutMode.PerMonitor,
            DefaultWallpaperId = wallpaper.Id
        });

        IReadOnlyList<WallpaperSession> sessions = await controller.BuildSessionsAsync(new AppSettings
        {
            LayoutMode = WallpaperLayoutMode.PerMonitor,
            DefaultWallpaperId = wallpaper.Id,
            Playback = new PlaybackProfile()
        }, CancellationToken.None);

        Assert.Equal(2, sessions.Count);
        Assert.All(sessions, session => Assert.Equal(wallpaper.Id, session.WallpaperId));
    }

    [Fact]
    public async Task BuildSessionsAsync_ReturnsSingleSpanSession_WhenSpanModeIsEnabled()
    {
        FakeWallpaperRepository wallpaperRepository = new();
        WallpaperMetadata wallpaper = wallpaperRepository.AddWallpaper("Span");
        FakeMonitorProvider monitorProvider = new([
            new MonitorDescriptor { DeviceName = "DISPLAY1", FriendlyName = "Display 1", Left = 0, Top = 0, Width = 1920, Height = 1080, IsPrimary = true },
            new MonitorDescriptor { DeviceName = "DISPLAY2", FriendlyName = "Display 2", Left = 1920, Top = 0, Width = 2560, Height = 1440, IsPrimary = false }
        ]);

        PlaybackController controller = CreatePlaybackController(wallpaperRepository, monitorProvider, new AppSettings
        {
            LayoutMode = WallpaperLayoutMode.Span,
            SpanWallpaperId = wallpaper.Id
        });

        IReadOnlyList<WallpaperSession> sessions = await controller.BuildSessionsAsync(new AppSettings
        {
            LayoutMode = WallpaperLayoutMode.Span,
            SpanWallpaperId = wallpaper.Id,
            Playback = new PlaybackProfile()
        }, CancellationToken.None);

        WallpaperSession session = Assert.Single(sessions);
        Assert.True(session.SpanAcrossAllMonitors);
        Assert.Equal(4480, session.TargetMonitor.Width);
        Assert.Equal(1440, session.TargetMonitor.Height);
    }

    private static PlaybackController CreatePlaybackController(
        FakeWallpaperRepository wallpaperRepository,
        FakeMonitorProvider monitorProvider,
        AppSettings settings)
    {
        FakeSettingsStore settingsStore = new(settings);
        SettingsService settingsService = new(settingsStore, new FakeStartupManager(), NullLogger<SettingsService>.Instance);
        MonitorManager monitorManager = new(monitorProvider);
        WallpaperManager wallpaperManager = new(wallpaperRepository, NullLogger<WallpaperManager>.Instance);

        return new PlaybackController(
            monitorManager,
            wallpaperManager,
            settingsService,
            new FakeVideoPlaybackEngine(),
            NullLogger<PlaybackController>.Instance);
    }

    private sealed class FakeWallpaperRepository : IWallpaperRepository
    {
        private readonly List<WallpaperMetadata> _items = [];

        public WallpaperMetadata AddWallpaper(string name)
        {
            WallpaperMetadata wallpaper = new()
            {
                DisplayName = name,
                RelativePath = $"assets\\wallpapers\\{name}.mp4",
                FileName = $"{name}.mp4",
                MediaType = WallpaperMediaType.Mp4
            };

            _items.Add(wallpaper);
            return wallpaper;
        }

        public Task<IReadOnlyList<WallpaperMetadata>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WallpaperMetadata>>(_items.ToArray());
        }

        public Task<WallpaperMetadata?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
        }

        public Task<WallpaperMetadata> ImportAsync(string sourcePath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string ResolveAbsolutePath(WallpaperMetadata wallpaper)
        {
            return wallpaper.RelativePath;
        }
    }

    private sealed class FakeMonitorProvider : IMonitorProvider
    {
        private readonly IReadOnlyList<MonitorDescriptor> _monitors;

        public FakeMonitorProvider(IReadOnlyList<MonitorDescriptor> monitors)
        {
            _monitors = monitors;
        }

        public Task<IReadOnlyList<MonitorDescriptor>> GetMonitorsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_monitors);
        }
    }

    private sealed class FakeSettingsStore : ISettingsStore
    {
        private AppSettings _settings;

        public FakeSettingsStore(AppSettings settings)
        {
            _settings = settings;
        }

        public event EventHandler<AppSettings>? SettingsChanged;

        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_settings.DeepCopy());
        }

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
        {
            _settings = settings.DeepCopy();
            SettingsChanged?.Invoke(this, _settings.DeepCopy());
            return Task.CompletedTask;
        }
    }

    private sealed class FakeStartupManager : IStartupManager
    {
        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task SetEnabledAsync(bool enabled, string executablePath, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeVideoPlaybackEngine : IVideoPlaybackEngine
    {
        public Task ApplyAsync(IReadOnlyList<WallpaperSession> sessions, CancellationToken cancellationToken) => Task.CompletedTask;
        public PlaybackRuntimeState GetState() => new();
        public Task PauseAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task RefreshDesktopBindingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task ResumeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
