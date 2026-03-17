using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace WallpaperUI;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Auto-start WallpaperEngine if it's not already running
        string enginePath = Path.Combine(AppContext.BaseDirectory, "WallpaperEngine.exe");
        if (File.Exists(enginePath))
        {
            var runningEngines = Process.GetProcessesByName("WallpaperEngine");
            if (runningEngines.Length == 0)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = enginePath,
                        UseShellExecute = true,
                        CreateNoWindow = true
                    });
                }
                catch { }
            }
        }
    }
}
