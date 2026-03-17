using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;
using WallpaperEngine.Infrastructure.Win32;

namespace WallpaperEngine.Infrastructure.Fullscreen;

public sealed class WindowsFullscreenStateProvider : IFullscreenStateProvider
{
    private static readonly HashSet<string> ShellWindowClasses = ["Progman", "WorkerW", "Shell_TrayWnd"];

    public Task<FullscreenInfo> GetFullscreenInfoAsync(CancellationToken cancellationToken)
    {
        IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero || NativeMethods.IsIconic(foregroundWindow))
        {
            return Task.FromResult(new FullscreenInfo());
        }

        string className = GetClassName(foregroundWindow);
        if (ShellWindowClasses.Contains(className))
        {
            return Task.FromResult(new FullscreenInfo());
        }

        if (!NativeMethods.GetWindowRect(foregroundWindow, out NativeMethods.Rect windowRect))
        {
            return Task.FromResult(new FullscreenInfo());
        }

        IntPtr monitorHandle = NativeMethods.MonitorFromWindow(foregroundWindow, NativeMethods.MonitorDefaultToNearest);
        if (monitorHandle == IntPtr.Zero)
        {
            return Task.FromResult(new FullscreenInfo());
        }

        NativeMethods.MonitorInfoEx monitorInfo = new()
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.MonitorInfoEx>()
        };

        if (!NativeMethods.GetMonitorInfo(monitorHandle, ref monitorInfo))
        {
            return Task.FromResult(new FullscreenInfo());
        }

        bool isFullscreen =
            windowRect.Left <= monitorInfo.rcMonitor.Left &&
            windowRect.Top <= monitorInfo.rcMonitor.Top &&
            windowRect.Right >= monitorInfo.rcMonitor.Right &&
            windowRect.Bottom >= monitorInfo.rcMonitor.Bottom;

        if (!isFullscreen)
        {
            return Task.FromResult(new FullscreenInfo());
        }

        NativeMethods.GetWindowThreadProcessId(foregroundWindow, out uint processId);

        string processName = string.Empty;
        try
        {
            using Process process = Process.GetProcessById((int)processId);
            processName = process.ProcessName;
        }
        catch
        {
        }

        return Task.FromResult(new FullscreenInfo
        {
            IsFullscreen = true,
            ProcessName = processName,
            WindowTitle = GetWindowTitle(foregroundWindow)
        });
    }

    private static string GetWindowTitle(IntPtr windowHandle)
    {
        int length = NativeMethods.GetWindowTextLength(windowHandle);
        StringBuilder builder = new(length + 1);
        NativeMethods.GetWindowText(windowHandle, builder, builder.Capacity);
        return builder.ToString();
    }

    private static string GetClassName(IntPtr windowHandle)
    {
        StringBuilder builder = new(256);
        NativeMethods.GetClassName(windowHandle, builder, builder.Capacity);
        return builder.ToString();
    }
}
