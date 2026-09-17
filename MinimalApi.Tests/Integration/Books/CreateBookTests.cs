using System.Net;
using System.Net.Http.Json;
using MinimalApi.Models;
using MinimalApi.Tests.Helpers;
using Xunit;

namespace MinimalApi.Tests.Integration.Books;

public class CreateBookTests : IntegrationTestBase
{
    public CreateBookTests(MinimalApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_Returns201AndLocation()
    {
        var response = await Client.PostAsJsonAsync("/books", new Book
        {
            Name = "The Pragmatic Programmer",
            Color = "Orange"
        });
        var created = await response.Content.ReadFromJsonAsync<Book>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("The Pragmatic Programmer", created.Name);
        Assert.Equal("Orange", created.Color);
        Assert.Equal($"/books/{created.Id}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Create_ThenGetById_ReturnsPersistedBook()
    {
        var createResponse = await Client.PostAsJsonAsync("/books", new Book
        {
            Name = "Working Effectively with Legacy Code",
            Color = "Brown"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<Book>();

        var book = await Client.GetFromJsonAsync<Book>($"/books/{created!.Id}");

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(book);
        Assert.Equal(created.Id, book.Id);
        Assert.Equal("Working Effectively with Legacy Code", book.Name);
        Assert.Equal("Brown", book.Color);
    }
}
