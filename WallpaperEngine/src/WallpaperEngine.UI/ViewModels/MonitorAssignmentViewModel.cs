using System.Collections.ObjectModel;

namespace WallpaperEngine.UI.ViewModels;

public sealed class MonitorAssignmentViewModel : ViewModelBase
{
    private WallpaperOptionViewModel? _selectedWallpaper;

    public string DeviceName { get; init; } = string.Empty;

    public string FriendlyName { get; init; } = string.Empty;

    public ObservableCollection<WallpaperOptionViewModel> AvailableWallpapers { get; } = [];

    public WallpaperOptionViewModel? SelectedWallpaper
    {
        get => _selectedWallpaper;
        set => SetProperty(ref _selectedWallpaper, value);
    }
}
