using System.Net;
using MinimalApi.Tests.Helpers;
using Xunit;

namespace MinimalApi.Tests.Integration.Books;

public class DeleteBookTests : IntegrationTestBase
{
    public DeleteBookTests(MinimalApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Delete_WhenBookExists_Returns204AndThen404()
    {
        var seeded = await SeedAsync("To Delete", "Yellow");

        var deleteResponse = await Client.DeleteAsync($"/books/{seeded.Id}");
        var getResponse = await Client.GetAsync($"/books/{seeded.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenBookDoesNotExist_Returns404()
    {
        var response = await Client.DeleteAsync("/books/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
