using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using WallpaperCore;

namespace WallpaperScreensaver;

public partial class App : Application
{
    [DllImport("user32.dll")]
    static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private HwndSource _winWPFContent;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string[] args = e.Args;

        if (args.Length > 0)
        {
            string firstArg = args[0].ToLower().Trim();
            string secondArg = null;

            if (firstArg.Length > 2)
            {
                secondArg = firstArg.Substring(3).Trim();
                firstArg = firstArg.Substring(0, 2);
            }
            else if (args.Length > 1)
            {
                secondArg = args[1];
            }

            if (firstArg == "/c")
            {
                // Config mode
                ShowSetup();
                return;
            }
            else if (firstArg == "/p")
            {
                // Preview mode
                if (secondArg == null)
                {
                    Application.Current.Shutdown();
                    return;
                }
                IntPtr previewWndHandle = new IntPtr(long.Parse(secondArg));
                ShowPreview(previewWndHandle);
                return;
            }
            else if (firstArg == "/s")
            {
                // Fullscreen mode
                ShowScreensaver();
                return;
            }
        }
        
        // If no arguments, just run as settings or fullscreen
        ShowScreensaver();
    }

    private void ShowSetup()
    {
        // Simply launch the WallpaperUI project for settings
        try
        {
            Process.Start("WallpaperUI.exe");
        }
        catch { }
        Application.Current.Shutdown();
    }

    private void ShowScreensaver()
    {
        foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
        {
            MainWindow window = new MainWindow();
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = screen.Bounds.Left;
            window.Top = screen.Bounds.Top;
            window.Width = screen.Bounds.Width;
            window.Height = screen.Bounds.Height;
            window.Show();
        }
    }

    private void ShowPreview(IntPtr previewWndHandle)
    {
        RECT lpRect;
        bool bGetRect = GetClientRect(previewWndHandle, out lpRect);

        HwndSourceParameters p = new HwndSourceParameters();
        p.WindowStyle = 1073741824 | 268435456; // WS_CHILD | WS_VISIBLE
        p.SetPosition(0, 0);
        p.SetSize(lpRect.Right - lpRect.Left, lpRect.Bottom - lpRect.Top);
        p.ParentWindow = previewWndHandle;

        _winWPFContent = new HwndSource(p);
        
        // We'll use a new instance of our screensaver content for the preview
        var previewContent = new ScreensaverControl();
        _winWPFContent.RootVisual = previewContent;
    }
}
