using WallpaperEngine.Core.Models;

namespace WallpaperEngine.Core.Interfaces;

public interface ISystemTrayService : IDisposable
{
    void Initialize(SystemTrayContext context);

    void UpdateState(SystemTrayState state);
}
