using WallpaperEngine.Core.Interfaces;
using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Application.Services;

public sealed class MonitorManager
{
    private readonly IMonitorProvider _monitorProvider;

    public MonitorManager(IMonitorProvider monitorProvider)
    {
        _monitorProvider = monitorProvider;
    }

    public async Task<IReadOnlyList<MonitorDescriptor>> GetMonitorsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<MonitorDescriptor> monitors = await _monitorProvider.GetMonitorsAsync(cancellationToken);

        return monitors
            .OrderByDescending(monitor => monitor.IsPrimary)
            .ThenBy(monitor => monitor.Left)
            .ThenBy(monitor => monitor.Top)
            .ToArray();
    }
}
