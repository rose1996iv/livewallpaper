using Microsoft.Win32;
using WallpaperEngine.Core.Interfaces;

namespace WallpaperEngine.Infrastructure.Startup;

public sealed class RegistryStartupManager : IStartupManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "WallpaperEngine";

    public Task<bool> IsEnabledAsync(CancellationToken cancellationToken)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        bool enabled = key?.GetValue(ValueName) is string value && !string.IsNullOrWhiteSpace(value);
        return Task.FromResult(enabled);
    }

    public Task SetEnabledAsync(bool enabled, string executablePath, CancellationToken cancellationToken)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath);

        if (enabled)
        {
            key.SetValue(ValueName, $"\"{executablePath}\"");
        }
        else if (key.GetValue(ValueName) is not null)
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }

        return Task.CompletedTask;
    }
}
