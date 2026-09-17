using System.Net;
using System.Net.Http.Json;
using MinimalApi.Models;
using MinimalApi.Tests.Helpers;
using Xunit;

namespace MinimalApi.Tests.Integration.Books;

public class UpdateBookTests : IntegrationTestBase
{
    public UpdateBookTests(MinimalApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Update_WhenBookExists_Returns204AndPersistsChanges()
    {
        var seeded = await SeedAsync("Old Name", "Black");

        var response = await Client.PutAsJsonAsync($"/books/{seeded.Id}", new Book
        {
            Name = "New Name",
            Color = "White"
        });
        var updated = await Client.GetFromJsonAsync<Book>($"/books/{seeded.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal("White", updated.Color);
    }

    [Fact]
    public async Task Update_WhenBookDoesNotExist_Returns404()
    {
        var response = await Client.PutAsJsonAsync("/books/999", new Book
        {
            Name = "Missing",
            Color = "Gray"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
