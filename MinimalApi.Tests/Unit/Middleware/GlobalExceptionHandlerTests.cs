using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using MinimalApi.Middleware;
using MinimalApi.Tests.Helpers;
using Xunit;

namespace MinimalApi.Tests.Unit.Middleware;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenKeyNotFound_Returns404ProblemDetails()
    {
        var problem = await HandleAsync(new KeyNotFoundException("Book was not found"), Environments.Development);

        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Resource not found", problem.Title);
        Assert.Equal("Book was not found", problem.Detail);
        Assert.Equal("/books/5", problem.Instance);
    }

    [Fact]
    public async Task TryHandleAsync_WhenArgumentException_Returns400ProblemDetails()
    {
        var problem = await HandleAsync(new ArgumentException("Name is required"), Environments.Development);

        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Invalid request", problem.Title);
        Assert.Equal("Name is required", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_WhenUnhandledExceptionInDevelopment_Returns500WithExceptionMessage()
    {
        var problem = await HandleAsync(new InvalidOperationException("SQL connection failed"), Environments.Development);

        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("An unexpected error occurred", problem.Title);
        Assert.Equal("SQL connection failed", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_WhenUnhandledExceptionInProduction_HidesExceptionMessage()
    {
        var problem = await HandleAsync(new InvalidOperationException("SQL connection failed"), Environments.Production);

        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("An error occurred while processing your request.", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_AlwaysReturnsTrue()
    {
        var httpContext = CreateHttpContext();
        var handler = CreateHandler(Environments.Development);

        var handled = await handler.TryHandleAsync(httpContext, new Exception("fail"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }

    private static async Task<ProblemDetails> HandleAsync(Exception exception, string environmentName)
    {
        var httpContext = CreateHttpContext();
        var handler = CreateHandler(environmentName);

        await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        httpContext.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            httpContext.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(problem);
        return problem;
    }

    private static GlobalExceptionHandler CreateHandler(string environmentName) =>
        new(NullLogger<GlobalExceptionHandler>.Instance, new FakeHostEnvironment { EnvironmentName = environmentName });

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            Request = { Path = "/books/5" },
            Response = { Body = new MemoryStream() }
        };
    }
}
