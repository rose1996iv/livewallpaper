using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using LibVLCSharp.Shared;

namespace VideoWallpaper;

public partial class App : System.Windows.Application
{
    private NotifyIcon? _notifyIcon;
    private List<MainWindow> _wallpapers = new();
    private ControlPanelWindow? _controlPanel;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            Core.Initialize(); // Initialize VLC globally once
        }
        catch { }

        foreach (var screen in Screen.AllScreens)
        {
            var window = new MainWindow(screen);
            window.Show();
            _wallpapers.Add(window);
        }

        InitializeTrayIcon();
        
        // If wallpaper doesn't map to an existing file on first run, show the Control Panel immediately.
        string path = AppSettings.WallpaperPath;
        if (AppSettings.WallpaperMode == "Video" && (string.IsNullOrEmpty(path) || !File.Exists(path)))
        {
             OpenControlPanel();
        }
    }

    public void RefreshWallpapers()
    {
        foreach (var window in _wallpapers)
        {
            window.UpdateSettings();
        }
    }

    private void OpenControlPanel()
    {
        if (_controlPanel == null || !_controlPanel.IsLoaded)
        {
            _controlPanel = new ControlPanelWindow(this);
            _controlPanel.Show();
        }
        _controlPanel.Activate();
    }

    private void InitializeTrayIcon()
    {
        _notifyIcon = new NotifyIcon();
        
        try
        {
            string iconPath = Path.Combine(AppContext.BaseDirectory, "banner.png");
            if (File.Exists(iconPath))
            {
                using var bmp = new Bitmap(iconPath);
                _notifyIcon.Icon = System.Drawing.Icon.FromHandle(bmp.GetHicon());
            }
            else
            {
                _notifyIcon.Icon = SystemIcons.Application;
            }
        }
        catch
        {
            _notifyIcon.Icon = SystemIcons.Application;
        }

        _notifyIcon.Text = "VideoWallpaper";
        _notifyIcon.Visible = true;

        var contextMenu = new ContextMenuStrip();
        
        var panelItem = new ToolStripMenuItem("Open Control Panel");
        panelItem.Click += (s, ev) => OpenControlPanel();
        
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
                OpenControlPanel();
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        foreach (var w in _wallpapers)
        {
            w.Close();
        }

        base.OnExit(e);
    }
}
