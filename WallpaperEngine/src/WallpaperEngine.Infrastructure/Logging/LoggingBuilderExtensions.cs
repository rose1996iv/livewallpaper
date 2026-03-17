using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WallpaperEngine.Infrastructure.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddWallpaperEngineFileLogger(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
        return loggingBuilder;
    }
}
