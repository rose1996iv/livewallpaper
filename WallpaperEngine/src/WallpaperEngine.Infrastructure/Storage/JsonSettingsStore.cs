using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;
using WallpaperEngine.Infrastructure.Hosting;

namespace WallpaperEngine.Infrastructure.Storage;

public sealed class JsonSettingsStore : ISettingsStore, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly AppPaths _paths;
    private readonly ILogger<JsonSettingsStore> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly FileSystemWatcher _watcher;

    public JsonSettingsStore(AppPaths paths, ILogger<JsonSettingsStore> logger)
    {
        _paths = paths;
        _logger = logger;

        Directory.CreateDirectory(_paths.ConfigDirectory);

        _watcher = new FileSystemWatcher(_paths.ConfigDirectory, Path.GetFileName(_paths.SettingsFilePath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime
        };

        _watcher.Changed += OnWatcherChanged;
        _watcher.Created += OnWatcherChanged;
        _watcher.EnableRaisingEvents = true;
    }

    public event EventHandler<AppSettings>? SettingsChanged;

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await LoadInternalAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_paths.ConfigDirectory);
            string json = JsonSerializer.Serialize(settings, SerializerOptions);
            await File.WriteAllTextAsync(_paths.SettingsFilePath, json, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
        _gate.Dispose();
    }

    private async void OnWatcherChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            await Task.Delay(150);
            AppSettings settings = await LoadInternalAsync(CancellationToken.None);
            SettingsChanged?.Invoke(this, settings);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to reload settings after a file change.");
        }
    }

    private async Task<AppSettings> LoadInternalAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_paths.SettingsFilePath))
        {
            AppSettings defaults = new();
            string json = JsonSerializer.Serialize(defaults, SerializerOptions);
            await File.WriteAllTextAsync(_paths.SettingsFilePath, json, cancellationToken);
            return defaults;
        }

        string content = await File.ReadAllTextAsync(_paths.SettingsFilePath, cancellationToken);
        return JsonSerializer.Deserialize<AppSettings>(content, SerializerOptions) ?? new AppSettings();
    }
}
