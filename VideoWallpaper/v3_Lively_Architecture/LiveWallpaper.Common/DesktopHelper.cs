using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace LiveWallpaper.Engine;

public static class DesktopHelper
{
    private const uint SpawnWorkerMessage = 0x052C;
    private const uint SendMessageTimeoutNormal = 0x0000;
    private const int GwlExStyle = -20;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExAppWindow = 0x00040000;

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam,
        uint fuFlags,
        uint uTimeout,
        out IntPtr lpdwResult);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindowEx(
        IntPtr hWndParent,
        IntPtr hWndChildAfter,
        string? lpszClass,
        string? lpszWindow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    public static void AttachToDesktop(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        IntPtr progman = FindWindow("Progman", null);
        if (progman == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to find the Program Manager window.");
        }

        // This undocumented message asks Progman to materialize the hidden WorkerW
        // host that sits behind SHELLDLL_DefView. By parenting our borderless window
        // to that WorkerW, the video renders behind the desktop icons instead of as
        // a normal application window.
        SendMessageTimeout(
            progman,
            SpawnWorkerMessage,
            new IntPtr(0xD),
            IntPtr.Zero,
            SendMessageTimeoutNormal,
            1000,
            out _);

        SendMessageTimeout(
            progman,
            SpawnWorkerMessage,
            new IntPtr(0xD),
            new IntPtr(1),
            SendMessageTimeoutNormal,
            1000,
            out _);

        IntPtr workerw = FindWorkerW();
        IntPtr targetParent = workerw != IntPtr.Zero ? workerw : progman;

        IntPtr windowHandle = new WindowInteropHelper(window).EnsureHandle();
        if (SetParent(windowHandle, targetParent) == IntPtr.Zero && Marshal.GetLastWin32Error() != 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to attach the wallpaper window to the desktop host.");
        }
    }

    public static void HideFromAltTab(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        IntPtr windowHandle = new WindowInteropHelper(window).EnsureHandle();
        int extendedStyle = GetWindowLongPtr(windowHandle, GwlExStyle).ToInt32();
        extendedStyle |= WsExToolWindow;
        extendedStyle &= ~WsExAppWindow;

        IntPtr result = SetWindowLongPtr(windowHandle, GwlExStyle, new IntPtr(extendedStyle));
        if (result == IntPtr.Zero && Marshal.GetLastWin32Error() != 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to update the window style.");
        }
    }

    private static IntPtr FindWorkerW()
    {
        IntPtr workerw = IntPtr.Zero;

        EnumWindows((topLevelWindow, _) =>
        {
            IntPtr shellView = FindWindowEx(topLevelWindow, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (shellView == IntPtr.Zero)
            {
                return true;
            }

            workerw = FindWindowEx(IntPtr.Zero, topLevelWindow, "WorkerW", null);
            return workerw == IntPtr.Zero;
        }, IntPtr.Zero);

        return workerw;
    }

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(hWnd, nIndex)
            : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newLong)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, newLong)
            : new IntPtr(SetWindowLong32(hWnd, nIndex, newLong.ToInt32()));
    }
}
