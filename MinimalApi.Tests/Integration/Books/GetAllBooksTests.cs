using System.Net;
using System.Net.Http.Json;
using MinimalApi.Models;
using MinimalApi.Tests.Helpers;
using Xunit;

namespace MinimalApi.Tests.Integration.Books;

public class GetAllBooksTests : IntegrationTestBase
{
    public GetAllBooksTests(MinimalApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_WhenNoBooks_Returns200AndEmptyList()
    {
        var response = await Client.GetAsync("/books");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(books);
        Assert.Empty(books);
    }

    [Fact]
    public async Task GetAll_WhenBooksExist_Returns200AndAllBooks()
    {
        await SeedAsync("Clean Code", "Blue");
        await SeedAsync("Refactoring", "Green");

        var response = await Client.GetAsync("/books");
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(books);
        Assert.Equal(2, books.Count);
        Assert.Contains(books, book => book.Name == "Clean Code" && book.Color == "Blue");
    }

    [Fact]
    public async Task GetAll_AllowsCorsFromAnyOrigin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/books");
        request.Headers.Add("Origin", "http://localhost:3000");

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        Assert.Contains("*", origins);
    }
}
