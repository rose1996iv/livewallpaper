using System.IO;
using System.Collections.ObjectModel;
using WallpaperEngine.Application.Services;
using WallpaperEngine.UI.Commands;

namespace WallpaperEngine.UI.ViewModels;

public sealed class WallpaperLibraryViewModel : ViewModelBase
{
    private readonly WallpaperManager _wallpaperManager;
    private WallpaperOptionViewModel? _selectedWallpaper;
    private string _statusMessage = "Ready";

    public WallpaperLibraryViewModel(WallpaperManager wallpaperManager)
    {
        _wallpaperManager = wallpaperManager;
        RefreshCommand = new AsyncRelayCommand(InitializeAsync);
        RemoveCommand = new AsyncRelayCommand(RemoveSelectedAsync, () => SelectedWallpaper is not null);
    }

    public event EventHandler? LibraryChanged;

    public ObservableCollection<WallpaperOptionViewModel> Wallpapers { get; } = [];

    public WallpaperOptionViewModel? SelectedWallpaper
    {
        get => _selectedWallpaper;
        set
        {
            if (SetProperty(ref _selectedWallpaper, value))
            {
                RemoveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand RemoveCommand { get; }

    public async Task InitializeAsync()
    {
        IReadOnlyList<WallpaperOptionViewModel> wallpapers = (await _wallpaperManager.GetLibraryAsync(CancellationToken.None))
            .Select(WallpaperOptionViewModel.FromMetadata)
            .ToArray();

        Wallpapers.Clear();
        foreach (WallpaperOptionViewModel wallpaper in wallpapers)
        {
            Wallpapers.Add(wallpaper);
        }

        SelectedWallpaper = Wallpapers.FirstOrDefault();
        StatusMessage = $"{Wallpapers.Count} wallpaper(s) in library.";
    }

    public async Task ImportAsync(string filePath)
    {
        await _wallpaperManager.ImportWallpaperAsync(filePath, CancellationToken.None);
        await InitializeAsync();
        StatusMessage = $"Imported {Path.GetFileName(filePath)}.";
        LibraryChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task RemoveSelectedAsync()
    {
        if (SelectedWallpaper is null)
        {
            return;
        }

        await _wallpaperManager.RemoveWallpaperAsync(SelectedWallpaper.Id, CancellationToken.None);
        await InitializeAsync();
        StatusMessage = "Wallpaper removed.";
        LibraryChanged?.Invoke(this, EventArgs.Empty);
    }
}
