using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using WallpaperEngine.Application.Services;
using WallpaperEngine.Core.Models;
using WallpaperEngine.UI.Commands;

namespace WallpaperEngine.UI.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly WallpaperManager _wallpaperManager;
    private readonly MonitorManager _monitorManager;
    private readonly PlaybackController _playbackController;
    private WallpaperLayoutMode _selectedLayoutMode;
    private WallpaperStretchMode _selectedStretchMode;
    private WallpaperOptionViewModel? _selectedDefaultWallpaper;
    private WallpaperOptionViewModel? _selectedSpanWallpaper;
    private bool _pauseOnFullscreen;
    private bool _startWithWindows;
    private bool _startPaused;
    private bool _useHardwareAcceleration = true;
    private bool _maintainAspectRatio = true;
    private bool _isMuted = true;
    private double _volume;
    private double _playbackSpeed = 1.0;
    private string _statusMessage = "Ready";

    public SettingsViewModel(
        SettingsService settingsService,
        WallpaperManager wallpaperManager,
        MonitorManager monitorManager,
        PlaybackController playbackController)
    {
        _settingsService = settingsService;
        _wallpaperManager = wallpaperManager;
        _monitorManager = monitorManager;
        _playbackController = playbackController;

        LayoutModes = Enum.GetValues<WallpaperLayoutMode>();
        StretchModes = Enum.GetValues<WallpaperStretchMode>();
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        RefreshCommand = new AsyncRelayCommand(InitializeAsync);
        OpenLibraryCommand = new RelayCommand(() => OpenLibraryRequested?.Invoke(this, EventArgs.Empty));
    }

    public event EventHandler? OpenLibraryRequested;

    public event EventHandler? LibraryRefreshRequested;

    public event EventHandler? SettingsApplied;

    public IReadOnlyList<WallpaperLayoutMode> LayoutModes { get; }

    public IReadOnlyList<WallpaperStretchMode> StretchModes { get; }

    public ObservableCollection<WallpaperOptionViewModel> Wallpapers { get; } = [];

    public ObservableCollection<MonitorAssignmentViewModel> MonitorAssignments { get; } = [];

    public WallpaperLayoutMode SelectedLayoutMode
    {
        get => _selectedLayoutMode;
        set
        {
            if (SetProperty(ref _selectedLayoutMode, value))
            {
                RaisePropertyChanged(nameof(IsPerMonitorMode));
                RaisePropertyChanged(nameof(IsSpanMode));
            }
        }
    }

    public WallpaperStretchMode SelectedStretchMode
    {
        get => _selectedStretchMode;
        set => SetProperty(ref _selectedStretchMode, value);
    }

    public WallpaperOptionViewModel? SelectedDefaultWallpaper
    {
        get => _selectedDefaultWallpaper;
        set => SetProperty(ref _selectedDefaultWallpaper, value);
    }

    public WallpaperOptionViewModel? SelectedSpanWallpaper
    {
        get => _selectedSpanWallpaper;
        set => SetProperty(ref _selectedSpanWallpaper, value);
    }

    public bool PauseOnFullscreen
    {
        get => _pauseOnFullscreen;
        set => SetProperty(ref _pauseOnFullscreen, value);
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetProperty(ref _startWithWindows, value);
    }

    public bool StartPaused
    {
        get => _startPaused;
        set => SetProperty(ref _startPaused, value);
    }

    public bool UseHardwareAcceleration
    {
        get => _useHardwareAcceleration;
        set => SetProperty(ref _useHardwareAcceleration, value);
    }

    public bool MaintainAspectRatio
    {
        get => _maintainAspectRatio;
        set => SetProperty(ref _maintainAspectRatio, value);
    }

    public bool IsMuted
    {
        get => _isMuted;
        set => SetProperty(ref _isMuted, value);
    }

    public double Volume
    {
        get => _volume;
        set => SetProperty(ref _volume, value);
    }

    public double PlaybackSpeed
    {
        get => _playbackSpeed;
        set => SetProperty(ref _playbackSpeed, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsPerMonitorMode => SelectedLayoutMode == WallpaperLayoutMode.PerMonitor;

    public bool IsSpanMode => SelectedLayoutMode == WallpaperLayoutMode.Span;

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public RelayCommand OpenLibraryCommand { get; }

    public async Task InitializeAsync()
    {
        IReadOnlyList<WallpaperMetadata> wallpapers = await _wallpaperManager.GetLibraryAsync(CancellationToken.None);
        IReadOnlyList<MonitorDescriptor> monitors = await _monitorManager.GetMonitorsAsync(CancellationToken.None);
        AppSettings settings = await _settingsService.GetCurrentAsync(CancellationToken.None);

        Wallpapers.Clear();
        foreach (WallpaperOptionViewModel wallpaper in wallpapers.Select(WallpaperOptionViewModel.FromMetadata))
        {
            Wallpapers.Add(wallpaper);
        }

        SelectedLayoutMode = settings.LayoutMode;
        SelectedStretchMode = settings.Playback.StretchMode;
        PauseOnFullscreen = settings.PauseOnFullscreen;
        StartWithWindows = settings.StartWithWindows;
        StartPaused = settings.StartPaused;
        UseHardwareAcceleration = settings.Playback.UseHardwareAcceleration;
        MaintainAspectRatio = settings.Playback.MaintainAspectRatio;
        IsMuted = settings.Playback.IsMuted;
        Volume = settings.Playback.Volume;
        PlaybackSpeed = settings.Playback.PlaybackSpeed;
        SelectedDefaultWallpaper = Wallpapers.FirstOrDefault(option => option.Id == settings.DefaultWallpaperId);
        SelectedSpanWallpaper = Wallpapers.FirstOrDefault(option => option.Id == settings.SpanWallpaperId);

        MonitorAssignments.Clear();
        foreach (MonitorDescriptor monitor in monitors)
        {
            MonitorAssignmentViewModel assignment = new()
            {
                DeviceName = monitor.DeviceName,
                FriendlyName = monitor.FriendlyName
            };

            foreach (WallpaperOptionViewModel wallpaper in Wallpapers)
            {
                assignment.AvailableWallpapers.Add(wallpaper);
            }

            Guid? assignedWallpaperId = settings.MonitorAssignments
                .FirstOrDefault(item => string.Equals(item.MonitorDeviceName, monitor.DeviceName, StringComparison.OrdinalIgnoreCase))
                ?.WallpaperId;

            assignment.SelectedWallpaper = assignment.AvailableWallpapers.FirstOrDefault(option => option.Id == assignedWallpaperId)
                ?? SelectedDefaultWallpaper
                ?? assignment.AvailableWallpapers.FirstOrDefault();

            MonitorAssignments.Add(assignment);
        }

        StatusMessage = $"Loaded {Wallpapers.Count} wallpapers across {MonitorAssignments.Count} monitor(s).";
        LibraryRefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    private async Task SaveAsync()
    {
        AppSettings settings = new()
        {
            LayoutMode = SelectedLayoutMode,
            DefaultWallpaperId = SelectedDefaultWallpaper?.Id,
            SpanWallpaperId = SelectedSpanWallpaper?.Id,
            PauseOnFullscreen = PauseOnFullscreen,
            StartWithWindows = StartWithWindows,
            StartPaused = StartPaused,
            Playback = new PlaybackProfile
            {
                Volume = Volume,
                PlaybackSpeed = PlaybackSpeed,
                IsMuted = IsMuted,
                UseHardwareAcceleration = UseHardwareAcceleration,
                MaintainAspectRatio = MaintainAspectRatio,
                StretchMode = SelectedStretchMode
            },
            MonitorAssignments = MonitorAssignments
                .Select(assignment => new MonitorWallpaperAssignment
                {
                    MonitorDeviceName = assignment.DeviceName,
                    WallpaperId = assignment.SelectedWallpaper?.Id
                })
                .ToList()
        };

        string executablePath = GetStartupExecutablePath();
        await _settingsService.SaveAsync(settings, executablePath, CancellationToken.None);
        await _playbackController.ApplyCurrentWallpaperAsync(CancellationToken.None);
        StatusMessage = "Settings saved and wallpaper playback updated.";
        SettingsApplied?.Invoke(this, EventArgs.Empty);
    }

    private static string GetStartupExecutablePath()
    {
        string? processPath = Process.GetCurrentProcess().MainModule?.FileName;
        if (!string.IsNullOrWhiteSpace(processPath))
        {
            return processPath;
        }

        return Environment.ProcessPath
            ?? Path.Combine(AppContext.BaseDirectory, "WallpaperEngine.UI.exe");
    }
}
