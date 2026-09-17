using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using MinimalApi.Tests.Helpers;
using Xunit;

namespace MinimalApi.Tests.Integration.Middleware;

public class GlobalExceptionHandlerTests : IntegrationTestBase
{
    public GlobalExceptionHandlerTests(MinimalApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task NotFoundException_Returns404ProblemDetails()
    {
        var response = await Client.GetAsync("/test/not-found");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Resource not found", problem.Title);
        Assert.Equal("Book was not found", problem.Detail);
        Assert.Equal("/test/not-found", problem.Instance);
    }

    [Fact]
    public async Task ArgumentException_Returns400ProblemDetails()
    {
        var response = await Client.GetAsync("/test/bad-request");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Invalid request", problem.Title);
        Assert.Equal("Name is required", problem.Detail);
    }

    [Fact]
    public async Task UnhandledExceptionInDevelopment_Returns500WithExceptionMessage()
    {
        var response = await Client.GetAsync("/test/server-error");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("An unexpected error occurred", problem.Title);
        Assert.Equal("SQL connection failed", problem.Detail);
    }
}

public class ProductionGlobalExceptionHandlerTests : IClassFixture<ProductionApiFactory>
{
    private readonly HttpClient _client;

    public ProductionGlobalExceptionHandlerTests(ProductionApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task UnhandledExceptionInProduction_HidesExceptionMessage()
    {
        var response = await _client.GetAsync("/test/server-error");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("An error occurred while processing your request.", problem.Detail);
    }
}
