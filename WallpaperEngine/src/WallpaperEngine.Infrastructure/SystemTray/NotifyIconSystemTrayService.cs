using System.Drawing;
using System.Windows.Forms;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Infrastructure.SystemTray;

public sealed class NotifyIconSystemTrayService : ISystemTrayService
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _togglePlaybackMenuItem;
    private SystemTrayContext? _context;

    public NotifyIconSystemTrayService()
    {
        _togglePlaybackMenuItem = new ToolStripMenuItem("Pause");

        ContextMenuStrip contextMenu = new();
        contextMenu.Items.Add(new ToolStripMenuItem("Open Settings", null, (_, _) => FireAndForget(_context?.OpenSettingsAsync)));
        contextMenu.Items.Add(new ToolStripMenuItem("Wallpaper Library", null, (_, _) => FireAndForget(_context?.OpenLibraryAsync)));
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(_togglePlaybackMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, (_, _) => FireAndForget(_context?.ExitAsync)));

        _togglePlaybackMenuItem.Click += (_, _) => FireAndForget(_context?.TogglePlaybackAsync);

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Wallpaper Engine",
            Visible = false,
            ContextMenuStrip = contextMenu
        };

        _notifyIcon.DoubleClick += (_, _) => FireAndForget(_context?.OpenSettingsAsync);
    }

    public void Initialize(SystemTrayContext context)
    {
        _context = context;
        _notifyIcon.Visible = true;
    }

    public void UpdateState(SystemTrayState state)
    {
        _notifyIcon.Text = TrimTooltip($"{state.TooltipText}: {state.ActiveWallpaperSummary}");
        _togglePlaybackMenuItem.Text = state.IsPaused ? "Resume" : "Pause";
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }

    private static void FireAndForget(Func<Task>? callback)
    {
        if (callback is null)
        {
            return;
        }

        _ = callback();
    }

    private static string TrimTooltip(string value)
    {
        return value.Length <= 63 ? value : value[..63];
    }
}
