using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell;
using Wpf.Ui.Controls;
using LiveWallpaper.Common;

namespace LiveWallpaper.UI;

public partial class GalleryWindow : FluentWindow
{
    private readonly string _wallpapersDir;

    public GalleryWindow()
    {
        InitializeComponent();
        Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
        
        _wallpapersDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "LiveWallpapers");
        if (!Directory.Exists(_wallpapersDir))
        {
            Directory.CreateDirectory(_wallpapersDir);
        }

        LoadWallpapers();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadWallpapers();
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = _wallpapersDir,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    private void LoadWallpapers()
    {
        var files = Directory.GetFiles(_wallpapersDir, "*.*")
            .Where(f => f.EndsWith(".mp4") || f.EndsWith(".mkv") || f.EndsWith(".mov") || f.EndsWith(".html"))
            .ToList();

        var items = new List<WallpaperItem>();

        foreach (var file in files)
        {
            BitmapSource? thumb = null;
            try
            {
                if (file.EndsWith(".html"))
                {
                    // No video thumbnail for html
                }
                else
                {
                    var shellFile = ShellFile.FromFilePath(file);
                    thumb = shellFile.Thumbnail.ExtraLargeBitmapSource;
                }
            }
            catch { }

            items.Add(new WallpaperItem
            {
                Name = Path.GetFileName(file),
                FilePath = file,
                Thumbnail = thumb
            });
        }

        GalleryItemsControl.ItemsSource = items;
    }

    private void Thumbnail_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is WallpaperItem item)
        {
            AppSettings.WallpaperPath = item.FilePath;
            AppSettings.WallpaperMode = item.FilePath.EndsWith(".html") ? "Web" : "Video";
            AppSettings.NotifySettingsChanged();
        }
    }
}

public class WallpaperItem
{
    public string Name { get; set; } = "";
    public string FilePath { get; set; } = "";
    public BitmapSource? Thumbnail { get; set; }
}
