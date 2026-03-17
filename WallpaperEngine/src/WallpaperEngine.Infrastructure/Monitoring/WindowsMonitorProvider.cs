using System.Windows.Forms;
using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Infrastructure.Monitoring;

public sealed class WindowsMonitorProvider : IMonitorProvider
{
    public Task<IReadOnlyList<MonitorDescriptor>> GetMonitorsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<MonitorDescriptor> monitors = Screen.AllScreens
            .Select(screen => new MonitorDescriptor
            {
                DeviceName = screen.DeviceName,
                FriendlyName = screen.DeviceName,
                Left = screen.Bounds.Left,
                Top = screen.Bounds.Top,
                Width = screen.Bounds.Width,
                Height = screen.Bounds.Height,
                IsPrimary = screen.Primary
            })
            .ToArray();

        return Task.FromResult(monitors);
    }
}
