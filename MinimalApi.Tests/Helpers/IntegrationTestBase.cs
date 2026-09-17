using Microsoft.Extensions.DependencyInjection;
using MinimalApi.Data;
using MinimalApi.Models;
using Xunit;

namespace MinimalApi.Tests.Helpers;

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<MinimalApiFactory>
{
}

[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly MinimalApiFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(MinimalApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Books.RemoveRange(db.Books);
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<Book> SeedAsync(string name, string color)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var book = new Book { Name = name, Color = color };
        db.Books.Add(book);
        await db.SaveChangesAsync();
        return book;
    }

    protected string ReadTodayLog()
    {
        var filePath = Path.Combine(Factory.LogPath, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
        Assert.True(File.Exists(filePath), $"Log file was not created: {filePath}");
        return File.ReadAllText(filePath);
    }
}
