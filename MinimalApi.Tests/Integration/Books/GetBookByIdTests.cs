using System.Net;
using System.Net.Http.Json;
using MinimalApi.Models;
using MinimalApi.Tests.Helpers;
using Xunit;

namespace MinimalApi.Tests.Integration.Books;

public class GetBookByIdTests : IntegrationTestBase
{
    public GetBookByIdTests(MinimalApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetById_WhenBookExists_Returns200()
    {
        var seeded = await SeedAsync("Domain Driven Design", "Red");

        var response = await Client.GetAsync($"/books/{seeded.Id}");
        var book = await response.Content.ReadFromJsonAsync<Book>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(book);
        Assert.Equal(seeded.Id, book.Id);
        Assert.Equal("Domain Driven Design", book.Name);
        Assert.Equal("Red", book.Color);
    }

    [Fact]
    public async Task GetById_WhenBookDoesNotExist_Returns404()
    {
        var response = await Client.GetAsync("/books/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
