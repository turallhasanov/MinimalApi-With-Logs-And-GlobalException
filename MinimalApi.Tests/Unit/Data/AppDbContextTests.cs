using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Models;
using Xunit;

namespace MinimalApi.Tests.Unit.Data;

public class AppDbContextTests : IDisposable
{
    private readonly AppDbContext _db = new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    [Fact]
    public async Task GetAll_WhenNoBooks_ReturnsEmptyList()
    {
        var books = await _db.Books.ToListAsync();

        Assert.Empty(books);
    }

    [Fact]
    public async Task Create_AddsBookAndGeneratesId()
    {
        var book = new Book { Name = "The Pragmatic Programmer", Color = "Orange" };

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        Assert.True(book.Id > 0);
        Assert.Equal(1, await _db.Books.CountAsync());
    }

    [Fact]
    public async Task GetById_WhenBookExists_ReturnsBook()
    {
        var seeded = await SeedAsync("Domain Driven Design", "Red");

        var book = await _db.Books.FindAsync(seeded.Id);

        Assert.NotNull(book);
        Assert.Equal("Domain Driven Design", book.Name);
        Assert.Equal("Red", book.Color);
    }

    [Fact]
    public async Task GetById_WhenBookDoesNotExist_ReturnsNull()
    {
        var book = await _db.Books.FindAsync(999);

        Assert.Null(book);
    }

    [Fact]
    public async Task Update_WhenBookExists_ChangesNameAndColor()
    {
        var seeded = await SeedAsync("Old Name", "Black");

        var existing = await _db.Books.FindAsync(seeded.Id);
        existing!.Name = "New Name";
        existing.Color = "White";
        await _db.SaveChangesAsync();

        var updated = await _db.Books.FindAsync(seeded.Id);
        Assert.Equal("New Name", updated!.Name);
        Assert.Equal("White", updated.Color);
    }

    [Fact]
    public async Task Delete_WhenBookExists_RemovesBook()
    {
        var seeded = await SeedAsync("To Delete", "Yellow");

        var existing = await _db.Books.FindAsync(seeded.Id);
        _db.Books.Remove(existing!);
        await _db.SaveChangesAsync();

        Assert.Null(await _db.Books.FindAsync(seeded.Id));
        Assert.Empty(_db.Books);
    }

    private async Task<Book> SeedAsync(string name, string color)
    {
        var book = new Book { Name = name, Color = color };
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return book;
    }

    public void Dispose() => _db.Dispose();
}
