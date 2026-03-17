using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using WallpaperCore;

namespace WallpaperEngine;

public partial class App : System.Windows.Application
{
    private List<MainWindow> _wallpapers = new();
    private EventWaitHandle? _settingsChangedEvent;
    private bool _isRunning = true;
    private NotifyIcon? _notifyIcon;
    private System.Threading.Timer? _autoRotateTimer;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            Core.Initialize(); // Initialize VLC globally once
        }
        catch { }

        // Spawn a wallpaper window for each screen!
        foreach (var screen in Screen.AllScreens)
        {
            var window = new MainWindow(screen);
            window.Show();
            _wallpapers.Add(window);
        }

        InitializeTrayIcon();

        // Setup auto rotation
        _autoRotateTimer = new System.Threading.Timer(AutoRotateCheck, null, 0, 60000); // Check every minute

        // Setup event listener for UI
        Task.Run(() => ListenForSettingsChanges());
    }

    private void AutoRotateCheck(object? state)
    {
        if (AppSettings.AutoRotate)
        {
            // Simple rotation: check last edit time or just use a shared timer.
            // A more robust approach checks if X minutes have passed since the file was set.
            // But since this timer ticks every minute, we can keep a static counter.
            // Wait, since we are a background process, we know we tick every minute.
            _rotationCounter++;
            if (_rotationCounter >= AppSettings.AutoRotateInterval)
            {
                _rotationCounter = 0;
                SelectNextWallpaper();
            }
        }
        else
        {
            _rotationCounter = 0;
        }
    }

    private int _rotationCounter = 0;

    private void SelectNextWallpaper()
    {
        try
        {
            string dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "LiveWallpapers");
            if (System.IO.Directory.Exists(dir))
            {
                var files = System.IO.Directory.GetFiles(dir, "*.*")
                    .Where(f => f.EndsWith(".mp4") || f.EndsWith(".mkv") || f.EndsWith(".mov") || f.EndsWith(".html"))
                    .ToList();
                
                if (files.Count > 1)
                {
                    // Pick next or random
                    var random = new Random();
                    string current = AppSettings.WallpaperPath;
                    string next = current;
                    while (next == current)
                    {
                        next = files[random.Next(files.Count)];
                    }
                    
                    AppSettings.WallpaperPath = next;
                    AppSettings.WallpaperMode = next.EndsWith(".html") ? "Web" : "Video";
                    AppSettings.NotifySettingsChanged();
                }
            }
        }
        catch { }
    }

    private void InitializeTrayIcon()
    {
        _notifyIcon = new NotifyIcon();
        
        try
        {
            string iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "banner.png");
            if (System.IO.File.Exists(iconPath))
            {
                using var bmp = new System.Drawing.Bitmap(iconPath);
                _notifyIcon.Icon = System.Drawing.Icon.FromHandle(bmp.GetHicon());
            }
            else
            {
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }
        }
        catch
        {
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
        }

        _notifyIcon.Text = "Wallpaper Engine";
        _notifyIcon.Visible = true;

        var contextMenu = new ContextMenuStrip();
        
        var panelItem = new ToolStripMenuItem("Open Dashboard");
        panelItem.Click += (s, ev) => OpenDashboard();
        
        var pausePlayItem = new ToolStripMenuItem("Play / Pause (Video)");
        pausePlayItem.Click += (s, ev) => 
        {
            foreach (var w in _wallpapers) w.TogglePlayPause();
        };
        
        var skipItem = new ToolStripMenuItem("Skip Next Wallpaper");
        skipItem.Click += (s, ev) => 
        {
            SelectNextWallpaper();
        };
        
        var muteItem = new ToolStripMenuItem("Mute / Unmute");
        muteItem.Click += (s, ev) => 
        {
            AppSettings.IsMuted = !AppSettings.IsMuted;
            AppSettings.NotifySettingsChanged();
        };
        
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, ev) => Shutdown();

        contextMenu.Items.Add(panelItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(pausePlayItem);
        contextMenu.Items.Add(skipItem);
        contextMenu.Items.Add(muteItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
        
        _notifyIcon.Click += (s, ev) => {
            if (ev is MouseEventArgs me && me.Button == MouseButtons.Left)
                OpenDashboard();
        };
    }

    private void OpenDashboard()
    {
        string uiPath = System.IO.Path.Combine(AppContext.BaseDirectory, "WallpaperUI.exe");
        if (System.IO.File.Exists(uiPath))
        {
            var runningUI = System.Diagnostics.Process.GetProcessesByName("WallpaperUI");
            if (runningUI.Length == 0)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = uiPath,
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }
    }

    private void ListenForSettingsChanges()
    {
        // Use Global scope so UI app can signal it
        bool createdNew;
        _settingsChangedEvent = new EventWaitHandle(false, EventResetMode.ManualReset, @"Global\VideoWallpaperSettingsChanged", out createdNew);

        while (_isRunning)
        {
            if (_settingsChangedEvent.WaitOne(1000))
            {
                _settingsChangedEvent.Reset();
                
                // Invoke on main thread to update settings
                Dispatcher.Invoke(() =>
                {
                    foreach (var w in _wallpapers)
                    {
                        w.UpdateSettings();
                    }
                });
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        _isRunning = false;
        _settingsChangedEvent?.Dispose();
        _autoRotateTimer?.Dispose();

        foreach (var w in _wallpapers)
        {
            w.Close();
        }

        base.OnExit(e);
    }
}
