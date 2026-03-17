using Microsoft.Extensions.Logging;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Application.Services;

public sealed class SettingsService
{
    private readonly ISettingsStore _settingsStore;
    private readonly IStartupManager _startupManager;
    private readonly ILogger<SettingsService> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private AppSettings? _cachedSettings;

    public SettingsService(
        ISettingsStore settingsStore,
        IStartupManager startupManager,
        ILogger<SettingsService> logger)
    {
        _settingsStore = settingsStore;
        _startupManager = startupManager;
        _logger = logger;

        _settingsStore.SettingsChanged += OnSettingsChanged;
    }

    public event EventHandler<AppSettings>? SettingsChanged;

    public async Task<AppSettings> GetCurrentAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_cachedSettings is null)
            {
                _cachedSettings = await _settingsStore.LoadAsync(cancellationToken);
                _cachedSettings.StartWithWindows = await _startupManager.IsEnabledAsync(cancellationToken);
            }

            return _cachedSettings.DeepCopy();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(AppSettings settings, string executablePath, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        AppSettings snapshot = settings.DeepCopy();

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await _settingsStore.SaveAsync(snapshot, cancellationToken);
            await _startupManager.SetEnabledAsync(snapshot.StartWithWindows, executablePath, cancellationToken);
            _cachedSettings = snapshot;
        }
        finally
        {
            _gate.Release();
        }

        _logger.LogInformation(
            "Saved settings. LayoutMode={LayoutMode}, PauseOnFullscreen={PauseOnFullscreen}, StartWithWindows={StartWithWindows}",
            snapshot.LayoutMode,
            snapshot.PauseOnFullscreen,
            snapshot.StartWithWindows);

        SettingsChanged?.Invoke(this, snapshot.DeepCopy());
    }

    private void OnSettingsChanged(object? sender, AppSettings settings)
    {
        _cachedSettings = settings.DeepCopy();
        _logger.LogInformation("Settings reloaded from disk.");
        SettingsChanged?.Invoke(this, settings.DeepCopy());
    }
}
