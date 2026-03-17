using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Core.Interfaces;

public interface IMonitorProvider
{
    Task<IReadOnlyList<MonitorDescriptor>> GetMonitorsAsync(CancellationToken cancellationToken);
}
