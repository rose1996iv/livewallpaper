using System.IO;
using Microsoft.Extensions.Logging;
using WallpaperEngine.Infrastructure.Hosting;

namespace WallpaperEngine.Infrastructure.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly object _syncRoot = new();

    public FileLoggerProvider(AppPaths paths)
    {
        Directory.CreateDirectory(paths.LogsDirectory);
        _filePath = paths.LogFilePath;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_filePath, categoryName, _syncRoot);
    }

    public void Dispose()
    {
    }

    private sealed class FileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly string _categoryName;
        private readonly object _syncRoot;

        public FileLogger(string filePath, string categoryName, object syncRoot)
        {
            _filePath = filePath;
            _categoryName = categoryName;
            _syncRoot = syncRoot;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string message = formatter(state, exception);
            string line = $"{DateTimeOffset.UtcNow:O} [{logLevel}] {_categoryName} {message}";
            if (exception is not null)
            {
                line = $"{line}{Environment.NewLine}{exception}";
            }

            lock (_syncRoot)
            {
                File.AppendAllText(_filePath, line + Environment.NewLine);
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
