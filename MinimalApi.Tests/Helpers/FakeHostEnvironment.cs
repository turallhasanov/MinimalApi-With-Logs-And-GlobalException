using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace MinimalApi.Tests.Helpers;

public class FakeHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Development;
    public string ApplicationName { get; set; } = "MinimalApi";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
