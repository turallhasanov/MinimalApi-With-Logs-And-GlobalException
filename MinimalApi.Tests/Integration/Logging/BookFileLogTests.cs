using System.Net;
using System.Net.Http.Json;
using MinimalApi.Models;
using MinimalApi.Tests.Helpers;
using Xunit;

namespace MinimalApi.Tests.Integration.Logging;

public class BookFileLogTests : IntegrationTestBase
{
    public BookFileLogTests(MinimalApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_WritesInformationAnd201ToDailyLog()
    {
        var response = await Client.PostAsJsonAsync("/books", new Book
        {
            Name = "Clean Code",
            Color = "Blue"
        });
        var created = await response.Content.ReadFromJsonAsync<Book>();
        var content = ReadTodayLog();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains("[Information] [201]", content);
        Assert.Contains($"Create book executed. StatusCode: 201, Id: {created!.Id}, Name: Clean Code", content);
    }

    [Fact]
    public async Task GetById_WhenMissing_WritesWarningAnd404ToDailyLog()
    {
        var response = await Client.GetAsync("/books/999");
        var content = ReadTodayLog();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("[Warning] [404]", content);
        Assert.Contains("GetById book executed. StatusCode: 404, Id: 999 not found", content);
    }

    [Fact]
    public async Task Update_WritesInformationAnd204ToSameDailyFile()
    {
        var seeded = await SeedAsync("Old Name", "Black");

        await Client.PostAsJsonAsync("/books", new Book { Name = "Another Book", Color = "Red" });
        await Client.PutAsJsonAsync($"/books/{seeded.Id}", new Book { Name = "New Name", Color = "White" });

        var files = Directory.GetFiles(Factory.LogPath, "log_*.txt");
        Assert.Single(files);
        Assert.Equal($"log_{DateTime.Now:yyyy-MM-dd}.txt", Path.GetFileName(files[0]));

        var content = File.ReadAllText(files[0]);
        Assert.Contains("[Information] [201]", content);
        Assert.Contains("[Information] [204]", content);
        Assert.Contains($"Update book executed. StatusCode: 204, Id: {seeded.Id}, Name: New Name", content);
    }
}
