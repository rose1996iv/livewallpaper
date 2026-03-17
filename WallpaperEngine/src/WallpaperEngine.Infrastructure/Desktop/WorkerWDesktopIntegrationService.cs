using System.ComponentModel;
using System.Runtime.InteropServices;
using WallpaperEngine.Infrastructure.Win32;

namespace WallpaperEngine.Infrastructure.Desktop;

public sealed class WorkerWDesktopIntegrationService
{
    public void AttachWindow(IntPtr windowHandle)
    {
        IntPtr progman = NativeMethods.FindWindow("Progman", null);
        if (progman == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to find the Program Manager window.");
        }

        // Sending the undocumented 0x052C message forces Explorer to materialize the
        // WorkerW host layer that sits behind SHELLDLL_DefView. Parenting our window to
        // that WorkerW keeps it beneath desktop icons while still visible as wallpaper.
        NativeMethods.SendMessageTimeout(
            progman,
            NativeMethods.SpawnWorkerMessage,
            new IntPtr(0xD),
            IntPtr.Zero,
            NativeMethods.SendMessageTimeoutNormal,
            1000,
            out _);

        NativeMethods.SendMessageTimeout(
            progman,
            NativeMethods.SpawnWorkerMessage,
            new IntPtr(0xD),
            new IntPtr(1),
            NativeMethods.SendMessageTimeoutNormal,
            1000,
            out _);

        IntPtr workerw = FindWorkerW();
        IntPtr targetParent = workerw != IntPtr.Zero ? workerw : progman;

        if (NativeMethods.SetParent(windowHandle, targetParent) == IntPtr.Zero && Marshal.GetLastWin32Error() != 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to parent the wallpaper window to the desktop host.");
        }
    }

    public void HideFromAltTab(IntPtr windowHandle)
    {
        int extendedStyle = GetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle).ToInt32();
        extendedStyle |= NativeMethods.WsExToolWindow;
        extendedStyle &= ~NativeMethods.WsExAppWindow;

        IntPtr result = SetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle, new IntPtr(extendedStyle));
        if (result == IntPtr.Zero && Marshal.GetLastWin32Error() != 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to hide the wallpaper window from Alt+Tab.");
        }
    }

    private static IntPtr FindWorkerW()
    {
        IntPtr workerw = IntPtr.Zero;

        NativeMethods.EnumWindows((topLevelWindow, _) =>
        {
            IntPtr shellView = NativeMethods.FindWindowEx(topLevelWindow, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (shellView == IntPtr.Zero)
            {
                return true;
            }

            workerw = NativeMethods.FindWindowEx(IntPtr.Zero, topLevelWindow, "WorkerW", null);
            return workerw == IntPtr.Zero;
        }, IntPtr.Zero);

        return workerw;
    }

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8
            ? NativeMethods.GetWindowLongPtr64(hWnd, nIndex)
            : new IntPtr(NativeMethods.GetWindowLong32(hWnd, nIndex));
    }

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newLong)
    {
        return IntPtr.Size == 8
            ? NativeMethods.SetWindowLongPtr64(hWnd, nIndex, newLong)
            : new IntPtr(NativeMethods.SetWindowLong32(hWnd, nIndex, newLong.ToInt32()));
    }
}
