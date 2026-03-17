using System;
using System.IO;
using System.Threading;
using Microsoft.Win32;

namespace WallpaperCore;

public static class AppSettings
{
    private const string KeyPath = @"SOFTWARE\VideoWallpaper";
    private const string SignalName = @"Global\VideoWallpaperSettingsChanged";

    public static void NotifySettingsChanged()
    {
        try
        {
            using var evt = new EventWaitHandle(false, EventResetMode.ManualReset, SignalName);
            evt.Set();
            evt.Reset();
        }
        catch { }
    }

    public static string WallpaperPath
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
            return key?.GetValue("WallpaperPath") as string ?? Path.Combine(AppContext.BaseDirectory, "wallpaper.mp4");
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
            key.SetValue("WallpaperPath", value);
        }
    }

    public static string WallpaperMode
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
            return key?.GetValue("WallpaperMode") as string ?? "Video";
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
            key.SetValue("WallpaperMode", value);
        }
    }

    // Since the Engine handles the rendering, let's keep AutoStart related to the Engine executable.
    // The UI can toggle it by passing its own path or Engine's path.
    // We will let UI toggle the Engine's auto start.
    public static void SetAutoStart(bool value, string engineExePath)
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        if (key != null)
        {
            if (value)
                key.SetValue("WallpaperEngine", $"\"{engineExePath}\"");
            else
                key.DeleteValue("WallpaperEngine", false);
        }
    }

    public static bool GetAutoStart()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
        return key?.GetValue("WallpaperEngine") != null;
    }

    public static bool IsMuted
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
            return (int)(key?.GetValue("IsMuted") ?? 1) == 1;
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
            key.SetValue("IsMuted", value ? 1 : 0);
        }
    }

    public static int Volume
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
            return (int)(key?.GetValue("Volume") ?? 100);
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
            key.SetValue("Volume", value);
        }
    }

    public static bool PauseOnBattery
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
            return (int)(key?.GetValue("PauseOnBattery") ?? 0) == 1;
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
            key.SetValue("PauseOnBattery", value ? 1 : 0);
        }
    }

    public static bool PauseOnFullscreen
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
            return (int)(key?.GetValue("PauseOnFullscreen") ?? 0) == 1;
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
            key.SetValue("PauseOnFullscreen", value ? 1 : 0);
        }
    }

    public static bool TransparentTaskbar
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
            return (int)(key?.GetValue("TransparentTaskbar") ?? 0) == 1;
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
            key.SetValue("TransparentTaskbar", value ? 1 : 0);
        }
    }

    public static bool AutoRotate
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
            return (int)(key?.GetValue("AutoRotate") ?? 0) == 1;
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
            key.SetValue("AutoRotate", value ? 1 : 0);
        }
    }

    public static int AutoRotateInterval
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
            return (int)(key?.GetValue("AutoRotateInterval") ?? 15);
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
            key.SetValue("AutoRotateInterval", value);
        }
    }
}
