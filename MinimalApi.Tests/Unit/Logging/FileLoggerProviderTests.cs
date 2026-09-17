using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MinimalApi.Logging;
using MinimalApi.Tests.Helpers;
using Xunit;

namespace MinimalApi.Tests.Unit.Logging;

public class FileLoggerProviderTests : IDisposable
{
    private readonly string _logDirectory = Path.Combine(Path.GetTempPath(), "MinimalApiUnitLogs", Guid.NewGuid().ToString());

    [Fact]
    public void Create_WritesInformationAnd201ToDailyTxt()
    {
        var logger = CreateLogger();

        logger.LogInformation("Create book executed. StatusCode: {StatusCode}, Id: {Id}, Name: {Name}", 201, 1, "Clean Code");

        var content = ReadTodayLog();
        Assert.Contains("[Information] [201]", content);
        Assert.Contains("Create book executed. StatusCode: 201, Id: 1, Name: Clean Code", content);
    }

    [Fact]
    public void GetByIdMissing_WritesWarningAnd404()
    {
        var logger = CreateLogger();

        logger.LogWarning("GetById book executed. StatusCode: {StatusCode}, Id: {Id} not found", 404, 99);

        var content = ReadTodayLog();
        Assert.Contains("[Warning] [404]", content);
        Assert.Contains("GetById book executed. StatusCode: 404, Id: 99 not found", content);
    }

    [Fact]
    public void UpdateAndCreate_AppendToSameDailyFile()
    {
        var logger = CreateLogger();

        logger.LogInformation("Create book executed. StatusCode: {StatusCode}", 201);
        logger.LogInformation("Update book executed. StatusCode: {StatusCode}, Id: {Id}", 204, 1);

        var files = Directory.GetFiles(_logDirectory, "log_*.txt");
        Assert.Single(files);
        Assert.Equal($"log_{DateTime.Now:yyyy-MM-dd}.txt", Path.GetFileName(files[0]));

        var content = File.ReadAllText(files[0]);
        Assert.Contains("[Information] [201]", content);
        Assert.Contains("[Information] [204]", content);
    }

    [Fact]
    public void RelativePath_IsResolvedFromContentRoot()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), "MinimalApiUnitContentRoot", Guid.NewGuid().ToString());
        Directory.CreateDirectory(contentRoot);

        using var provider = new FileLoggerProvider(
            Options.Create(new FileLoggerOptions
            {
                Path = "Logs",
                FileNamePattern = "log_{0:yyyy-MM-dd}.txt"
            }),
            new FakeHostEnvironment { ContentRootPath = contentRoot });

        provider.CreateLogger("Books").LogInformation("GetAll books executed. StatusCode: {StatusCode}, Count: {Count}", 200, 0);

        var expectedFile = Path.Combine(contentRoot, "Logs", $"log_{DateTime.Now:yyyy-MM-dd}.txt");
        Assert.True(File.Exists(expectedFile));
        Assert.Contains("[Information] [200]", File.ReadAllText(expectedFile));
    }

    private ILogger CreateLogger()
    {
        var provider = new FileLoggerProvider(
            Options.Create(new FileLoggerOptions
            {
                Path = _logDirectory,
                FileNamePattern = "log_{0:yyyy-MM-dd}.txt"
            }),
            new FakeHostEnvironment());

        return provider.CreateLogger("MinimalApi");
    }

    private string ReadTodayLog()
    {
        var filePath = Path.Combine(_logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
        Assert.True(File.Exists(filePath));
        return File.ReadAllText(filePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_logDirectory))
        {
            Directory.Delete(_logDirectory, recursive: true);
        }
    }
}
