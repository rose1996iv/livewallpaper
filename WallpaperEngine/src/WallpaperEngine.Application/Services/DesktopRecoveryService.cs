using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WallpaperEngine.Application.Services;

public sealed class DesktopRecoveryService : BackgroundService
{
    private readonly PlaybackController _playbackController;
    private readonly ILogger<DesktopRecoveryService> _logger;

    public DesktopRecoveryService(
        PlaybackController playbackController,
        ILogger<DesktopRecoveryService> logger)
    {
        _playbackController = playbackController;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await _playbackController.RefreshDesktopAttachmentAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to refresh desktop attachment.");
            }
        }
    }
}
