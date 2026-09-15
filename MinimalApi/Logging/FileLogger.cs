using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MinimalApi.Logging;

public class FileLoggerOptions
{
    public const string SectionName = "Logging:File";
    public string Path { get; set; } = "Logs";
    public string FileNamePattern { get; set; } = "log_{0:yyyy-MM-dd}.txt";
}

[ProviderAlias("File")]
public class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggerOptions _options;
    private readonly string _logDirectory;

    public FileLoggerProvider(IOptions<FileLoggerOptions> options, IHostEnvironment environment)
    {
        _options = options.Value;
        _logDirectory = Path.IsPathRooted(_options.Path)
            ? _options.Path
            : Path.Combine(environment.ContentRootPath, _options.Path);

        Directory.CreateDirectory(_logDirectory);
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, _logDirectory, _options);

    public void Dispose() { }
}

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, IConfiguration configuration)
    {
        builder.Services.Configure<FileLoggerOptions>(configuration.GetSection(FileLoggerOptions.SectionName));
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
        return builder;
    }
}

file class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logDirectory;
    private readonly FileLoggerOptions _options;
    private static readonly object Lock = new();

    public FileLogger(string categoryName, string logDirectory, FileLoggerOptions options)
    {
        _categoryName = categoryName;
        _logDirectory = logDirectory;
        _options = options;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        Directory.CreateDirectory(_logDirectory);

        var timestamp = DateTime.Now;
        var filePath = Path.Combine(_logDirectory, string.Format(_options.FileNamePattern, timestamp));
        var statusCode = GetStatusCode(state);
        var statusPart = statusCode is not null ? $" [{statusCode}]" : string.Empty;
        var content =
            $"[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}]{statusPart} {_categoryName}{Environment.NewLine}" +
            $"{formatter(state, exception)}{Environment.NewLine}";

        if (exception is not null)
        {
            content += $"{exception}{Environment.NewLine}";
        }

        content += new string('-', 80) + Environment.NewLine;

        lock (Lock)
        {
            File.AppendAllText(filePath, content);
        }
    }

    private static int? GetStatusCode<TState>(TState state)
    {
        if (state is not IEnumerable<KeyValuePair<string, object>> properties)
        {
            return null;
        }

        foreach (var property in properties)
        {
            if (property.Key == "StatusCode" && property.Value is int code)
            {
                return code;
            }
        }

        return null;
    }
}
