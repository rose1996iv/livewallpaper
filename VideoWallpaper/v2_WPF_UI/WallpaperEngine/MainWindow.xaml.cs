using System;
using System.IO;
using System.Windows;
using LibVLCSharp.Shared;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.Threading.Tasks;
using WallpaperCore;

namespace WallpaperEngine;

public partial class MainWindow : Window
{
    private LibVLC? _libVLC;
    private LibVLCSharp.Shared.MediaPlayer? _mediaPlayer;
    private Screen _targetScreen;
    private bool _isWebViewInitialized = false;
    private string _currentVideoPath = "";
    private string _currentWebUrl = "";
    private System.Windows.Threading.DispatcherTimer _resourceMonitorTimer;
    private System.Windows.Threading.DispatcherTimer _mouseForwardTimer;
    private bool _isPausedByResourceMonitor = false;
    private System.Windows.Point _lastMousePos;
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
        
        _resourceMonitorTimer = new System.Windows.Threading.DispatcherTimer();
        _resourceMonitorTimer.Interval = TimeSpan.FromSeconds(1);
        _resourceMonitorTimer.Tick += ResourceMonitorTimer_Tick;

        _mouseForwardTimer = new System.Windows.Threading.DispatcherTimer();
        _mouseForwardTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
        _mouseForwardTimer.Tick += MouseForwardTimer_Tick;
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
        _resourceMonitorTimer.Start();
    }

    private void ConfigureBounds()
    {
        Left = _targetScreen.Bounds.Left;
        Top = _targetScreen.Bounds.Top;
        Width = _targetScreen.Bounds.Width;
        Height = _targetScreen.Bounds.Height;
    }

    private void ResourceMonitorTimer_Tick(object? sender, EventArgs e)
    {
        bool shouldPause = false;

        // 1. Check Battery
        if (AppSettings.PauseOnBattery)
        {
            if (SystemInformation.PowerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Offline)
            {
                shouldPause = true;
            }
        }

        // 2. Check Fullscreen
        if (!shouldPause && AppSettings.PauseOnFullscreen)
        {
            IntPtr fgWindow = DesktopHelper.GetForegroundWindow();
            if (fgWindow != IntPtr.Zero)
            {
                // Ignore Desktop windows
                IntPtr progman = DesktopHelper.FindWindow("Progman", null);
                IntPtr workerw = DesktopHelper.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "WorkerW", null);
                if (fgWindow != progman && fgWindow != workerw)
                {
                    if (DesktopHelper.GetWindowRect(fgWindow, out DesktopHelper.RECT rect))
                    {
                        int w = rect.Right - rect.Left;
                        int h = rect.Bottom - rect.Top;
                        if (w >= _targetScreen.Bounds.Width && h >= _targetScreen.Bounds.Height)
                        {
                            shouldPause = true;
                        }
                    }
                }
            }
        }

        if (shouldPause)
        {
            if (!_isPausedByResourceMonitor)
            {
                _isPausedByResourceMonitor = true;
                if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Pause();
                }
            }
        }
        else
        {
            if (_isPausedByResourceMonitor)
            {
                _isPausedByResourceMonitor = false;
                if (_mediaPlayer != null && !_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Play();
                }
            }
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    private async void MouseForwardTimer_Tick(object? sender, EventArgs e)
    {
        if (WebPlayer.Visibility != Visibility.Visible || _isPausedByResourceMonitor) return;

        if (GetCursorPos(out POINT p))
        {
            // Only forward if mouse moved
            if (p.X != _lastMousePos.X || p.Y != _lastMousePos.Y)
            {
                _lastMousePos = new System.Windows.Point(p.X, p.Y);
                double scaledX = p.X - Left; // Adjust for screen position
                double scaledY = p.Y - Top;

                if (scaledX >= 0 && scaledX <= Width && scaledY >= 0 && scaledY <= Height)
                {
                    string script = $@"
                        var ev = new MouseEvent('mousemove', {{
                            'view': window,
                            'bubbles': true,
                            'cancelable': true,
                            'clientX': {scaledX},
                            'clientY': {scaledY}
                        }});
                        document.dispatchEvent(ev);
                    ";
                    try
                    {
                        await WebPlayer.CoreWebView2.ExecuteScriptAsync(script);
                    }
                    catch { }
                }
            }
        }
    }

    public async void UpdateSettings()
    {
        string mode = AppSettings.WallpaperMode;
        string path = AppSettings.WallpaperPath;
        
        TaskbarHelper.SetTaskbarTransparent(AppSettings.TransparentTaskbar);

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
            _mouseForwardTimer.Stop();
            
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
        _mouseForwardTimer.Start();
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
