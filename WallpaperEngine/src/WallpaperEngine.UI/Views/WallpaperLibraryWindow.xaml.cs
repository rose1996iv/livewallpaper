using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using WallpaperEngine.UI.ViewModels;

namespace WallpaperEngine.UI.Views;

public partial class WallpaperLibraryWindow : Window
{
    private readonly WallpaperLibraryViewModel _viewModel;

    public WallpaperLibraryWindow(WallpaperLibraryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        StateChanged += OnWindowStateChanged;
    }

    public bool AllowClose { get; set; }

    private async void AddWallpaper_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog dialog = new()
        {
            Filter = "Video Wallpapers (*.mp4;*.webm)|*.mp4;*.webm",
            CheckFileExists = true,
            Multiselect = false,
            Title = "Import wallpaper video"
        };

        if (dialog.ShowDialog(this) == true)
        {
            await _viewModel.ImportAsync(dialog.FileName);
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!AllowClose)
        {
            e.Cancel = true;
            Hide();
        }

        base.OnClosing(e);
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (!AllowClose && WindowState == WindowState.Minimized)
        {
            Hide();
        }
    }
}
