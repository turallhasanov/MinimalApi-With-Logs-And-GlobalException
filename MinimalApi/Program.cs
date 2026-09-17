using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Logging;
using MinimalApi.Middleware;
using MinimalApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFileLogger(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors();

if (app.Configuration.GetValue<bool>("EnableTestErrorRoutes"))
{
    app.MapGet("/test/not-found", () =>
    {
        throw new KeyNotFoundException("Book was not found");
    });
    app.MapGet("/test/bad-request", () =>
    {
        throw new ArgumentException("Name is required");
    });
    app.MapGet("/test/server-error", () =>
    {
        throw new InvalidOperationException("SQL connection failed");
    });
}

var log = app.Logger;

app.MapGet("/books", async (AppDbContext db) =>
{
    var books = await db.Books.ToListAsync();
    log.LogInformation("GetAll books executed. StatusCode: {StatusCode}, Count: {Count}", StatusCodes.Status200OK, books.Count);
    return Results.Ok(books);
})
.Produces<List<Book>>(StatusCodes.Status200OK);

app.MapGet("/books/{id:int}", async (int id, AppDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book is null)
    {
        log.LogWarning("GetById book executed. StatusCode: {StatusCode}, Id: {Id} not found", StatusCodes.Status404NotFound, id);
        return Results.NotFound();
    }

    log.LogInformation("GetById book executed. StatusCode: {StatusCode}, Id: {Id}", StatusCodes.Status200OK, id);
    return Results.Ok(book);
});

app.MapPost("/books", async (Book book, AppDbContext db) =>
{
    db.Books.Add(book);
    await db.SaveChangesAsync();
    log.LogInformation("Create book executed. StatusCode: {StatusCode}, Id: {Id}, Name: {Name}", StatusCodes.Status201Created, book.Id, book.Name);
    return Results.Created($"/books/{book.Id}", book);
});

app.MapPut("/books/{id:int}", async (int id, Book book, AppDbContext db) =>
{
    var existing = await db.Books.FindAsync(id);
    if (existing is null)
    {
        log.LogWarning("Update book executed. StatusCode: {StatusCode}, Id: {Id} not found", StatusCodes.Status404NotFound, id);
        return Results.NotFound();
    }

    existing.Name = book.Name;
    existing.Color = book.Color;
    await db.SaveChangesAsync();
    log.LogInformation("Update book executed. StatusCode: {StatusCode}, Id: {Id}, Name: {Name}", StatusCodes.Status204NoContent, id, existing.Name);
    return Results.NoContent();
});

app.MapDelete("/books/{id:int}", async (int id, AppDbContext db) =>
{
    var existing = await db.Books.FindAsync(id);
    if (existing is null)
    {
        log.LogWarning("Delete book executed. StatusCode: {StatusCode}, Id: {Id} not found", StatusCodes.Status404NotFound, id);
        return Results.NotFound();
    }

    db.Books.Remove(existing);
    await db.SaveChangesAsync();
    log.LogInformation("Delete book executed. StatusCode: {StatusCode}, Id: {Id}", StatusCodes.Status204NoContent, id);
    return Results.NoContent();
});

app.Run();

public partial class Program { }
