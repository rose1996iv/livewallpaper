using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WallpaperEngine.Core.Interfaces;

namespace WallpaperEngine.Application.Services;

public sealed class FullscreenDetector : BackgroundService
{
    private readonly IFullscreenStateProvider _fullscreenStateProvider;
    private readonly SettingsService _settingsService;
    private readonly PlaybackController _playbackController;
    private readonly ILogger<FullscreenDetector> _logger;

    public FullscreenDetector(
        IFullscreenStateProvider fullscreenStateProvider,
        SettingsService settingsService,
        PlaybackController playbackController,
        ILogger<FullscreenDetector> logger)
    {
        _fullscreenStateProvider = fullscreenStateProvider;
        _settingsService = settingsService;
        _playbackController = playbackController;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(2));
        bool? lastFullscreenState = null;

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var settings = await _settingsService.GetCurrentAsync(stoppingToken);
                if (!settings.PauseOnFullscreen)
                {
                    if (lastFullscreenState == true)
                    {
                        await _playbackController.SetPauseAsync(PauseReason.Fullscreen, false, stoppingToken);
                    }

                    lastFullscreenState = false;
                    continue;
                }

                var fullscreenInfo = await _fullscreenStateProvider.GetFullscreenInfoAsync(stoppingToken);
                if (fullscreenInfo.IsFullscreen == lastFullscreenState)
                {
                    continue;
                }

                await _playbackController.SetPauseAsync(PauseReason.Fullscreen, fullscreenInfo.IsFullscreen, stoppingToken);
                lastFullscreenState = fullscreenInfo.IsFullscreen;

                _logger.LogInformation(
                    "Fullscreen state changed. IsFullscreen={IsFullscreen}, Process={ProcessName}, Title={WindowTitle}",
                    fullscreenInfo.IsFullscreen,
                    fullscreenInfo.ProcessName,
                    fullscreenInfo.WindowTitle);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while monitoring fullscreen state.");
            }
        }
    }
}
