using System;
using System.IO;
using System.Windows;
using LibVLCSharp.Shared;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.Threading.Tasks;

namespace VideoWallpaper;

public partial class MainWindow : Window
{
    private LibVLC? _libVLC;
    private LibVLCSharp.Shared.MediaPlayer? _mediaPlayer;
    private Screen _targetScreen;
    private bool _isWebViewInitialized = false;
    private string _currentVideoPath = "";
    private string _currentWebUrl = "";
    public int Volume
    {
        get => _mediaPlayer?.Volume ?? 0;
        set { if (_mediaPlayer != null && _mediaPlayer.IsPlaying) { _mediaPlayer.Mute = false; _mediaPlayer.Volume = value; } }
    }

    public bool IsMuted
    {
        get => _mediaPlayer?.Mute ?? true;
        set { 
            if (_mediaPlayer != null) 
            { 
                _mediaPlayer.Mute = value; 
                if (_mediaPlayer.IsPlaying)
                {
                    if (value) _mediaPlayer.Volume = 0;
                    else _mediaPlayer.Volume = AppSettings.Volume;
                }
            }
        }
    }

    public MainWindow(Screen? targetScreen = null)
    {
        InitializeComponent();
        _targetScreen = targetScreen ?? Screen.PrimaryScreen ?? throw new Exception("No primary screen detected.");
    }

    private void Window_SourceInitialized(object? sender, EventArgs e)
    {
        try
        {
            ConfigureBounds();
            DesktopHelper.HideFromAltTab(this);
            DesktopHelper.AttachToDesktop(this);
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(
                $"Unable to initialize the desktop wallpaper host.{Environment.NewLine}{exception.Message}",
                "VideoWallpaper",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            System.Windows.Application.Current.Shutdown();
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateSettings();
    }

    private void ConfigureBounds()
    {
        Left = _targetScreen.Bounds.Left;
        Top = _targetScreen.Bounds.Top;
        Width = _targetScreen.Bounds.Width;
        Height = _targetScreen.Bounds.Height;
    }

    public async void UpdateSettings()
    {
        string mode = AppSettings.WallpaperMode;
        string path = AppSettings.WallpaperPath;

        if (string.IsNullOrEmpty(path)) 
        {
            StopVideo();
            WebPlayer.Visibility = Visibility.Collapsed;
            WallpaperPlayer.Visibility = Visibility.Collapsed;
            _currentVideoPath = "";
            _currentWebUrl = "";
            return; 
        }

        if (mode == "Video")
        {
            WebPlayer.Visibility = Visibility.Collapsed;
            WallpaperPlayer.Visibility = Visibility.Visible;
            if (_currentWebUrl != "") { _currentWebUrl = ""; }
            
            if (_currentVideoPath != path)
            {
                StartVideo(path);
            }
            else
            {
                // Just update volume/mute without restarting
                if (_mediaPlayer != null)
                {
                    if (AppSettings.IsMuted)
                    {
                        IsMuted = true;
                    }
                    else
                    {
                        Volume = AppSettings.Volume;
                        IsMuted = false;
                    }
                }
            }
        }
        else if (mode == "Web")
        {
            WallpaperPlayer.Visibility = Visibility.Collapsed;
            WebPlayer.Visibility = Visibility.Visible;
            StopVideo();
            _currentVideoPath = "";
            
            if (_currentWebUrl != path)
            {
                await StartWeb(path);
            }
        }
    }

    private void StartVideo(string videoPath)
    {
        if (!File.Exists(videoPath)) return;

        try
        {
            if (_libVLC == null)
            {
                _libVLC = new LibVLC("--input-repeat=65535", "--no-osd", "--avcodec-hw=d3d11va");
                _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
                _mediaPlayer.Playing += (s, e) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (AppSettings.IsMuted)
                        {
                            IsMuted = true;
                        }
                        else
                        {
                            Volume = AppSettings.Volume;
                            IsMuted = false;
                        }
                    });
                };
                WallpaperPlayer.MediaPlayer = _mediaPlayer;
            }
            
            _mediaPlayer?.Stop();
            using var media = new Media(_libVLC, new Uri(videoPath, UriKind.Absolute));
            _mediaPlayer?.Play(media);
            _currentVideoPath = videoPath;
        }
        catch { }
    }

    private void StopVideo()
    {
        _mediaPlayer?.Stop();
        _currentVideoPath = "";
    }

    private async Task StartWeb(string urlOrPath)
    {
        if (!_isWebViewInitialized)
        {
            var env = await CoreWebView2Environment.CreateAsync(null, Path.Combine(AppContext.BaseDirectory, "WebView2Data"));
            await WebPlayer.EnsureCoreWebView2Async(env);
            
            // disable context menus, zoom, etc for a wallpaper feel
            WebPlayer.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            WebPlayer.CoreWebView2.Settings.IsZoomControlEnabled = false;
            WebPlayer.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _isWebViewInitialized = true;
        }
        
        if (File.Exists(urlOrPath))
        {
            WebPlayer.CoreWebView2.Navigate($"file:///{urlOrPath.Replace("\\", "/")}");
        }
        else
        {
            if (!urlOrPath.StartsWith("http")) urlOrPath = "https://" + urlOrPath;
            WebPlayer.CoreWebView2.Navigate(urlOrPath);
        }
        _currentWebUrl = urlOrPath;
    }

    public void TogglePlayPause()
    {
        if (_mediaPlayer == null) return;
        
        if (_mediaPlayer.IsPlaying)
            _mediaPlayer.Pause();
        else
            _mediaPlayer.Play();
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Dispose();
        }

        _libVLC?.Dispose();
        WebPlayer?.Dispose();
    }
}
