using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalApi.Data;

namespace MinimalApi.Tests.Helpers;

public class MinimalApiFactory : WebApplicationFactory<Program>
{
    private readonly string _environment;
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public string LogPath { get; } = Path.Combine(Path.GetTempPath(), "MinimalApiTests", Guid.NewGuid().ToString());

    public MinimalApiFactory() : this(Environments.Development)
    {
    }

    protected MinimalApiFactory(string environment)
    {
        _environment = environment;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:File:Path"] = LogPath,
                ["EnableTestErrorRoutes"] = "true"
            });
        });
        builder.ConfigureServices(services =>
        {
            RemoveDbContext(services);
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    private static void RemoveDbContext(IServiceCollection services)
    {
        var descriptors = services.Where(descriptor =>
            descriptor.ServiceType == typeof(AppDbContext) ||
            descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
            (descriptor.ServiceType.IsGenericType &&
             descriptor.ServiceType.GetGenericArguments().Contains(typeof(AppDbContext)))).ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}

public sealed class ProductionApiFactory : MinimalApiFactory
{
    public ProductionApiFactory() : base(Environments.Production)
    {
    }
}
