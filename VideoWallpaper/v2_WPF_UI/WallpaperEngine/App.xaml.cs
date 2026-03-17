using System;
using System.Collections.Generic;
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

        // Setup event listener for UI
        Task.Run(() => ListenForSettingsChanges());
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
        
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, ev) => Shutdown();

        contextMenu.Items.Add(panelItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(pausePlayItem);
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

        foreach (var w in _wallpapers)
        {
            w.Close();
        }

        base.OnExit(e);
    }
}
