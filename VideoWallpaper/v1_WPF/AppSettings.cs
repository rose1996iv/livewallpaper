using System;
using System.IO;
using Microsoft.Win32;

namespace VideoWallpaper;

public static class AppSettings
{
    private const string KeyPath = @"SOFTWARE\VideoWallpaper";

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

    public static bool AutoStart
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            return key?.GetValue("VideoWallpaper") != null;
        }
        set
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key != null)
            {
                if (value)
                    key.SetValue("VideoWallpaper", $"\"{Environment.ProcessPath}\"");
                else
                    key.DeleteValue("VideoWallpaper", false);
            }
        }
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
}
