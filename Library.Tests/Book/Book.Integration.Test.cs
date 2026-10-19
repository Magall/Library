using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Library.Database;
using Library.Database.Models;
using Library.Tests.Book;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Tests;

public sealed class BookIntegrationTest : IClassFixture<ApiFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly LibraryDbContext _context;

    public BookIntegrationTest(ApiFactory factory)
    {
        _client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        _context.Books.ExecuteDelete();
        _context.Authors.ExecuteDelete();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }

    [Fact]
    public async Task GetBook_ReturnsBooksList()
    {

        _context.Books.AddRange(
            BookFactory.MockBook("Dune"),
            BookFactory.MockBook("Clean Code")
        );
        await _context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/books");
        var books = await response.Content.ReadFromJsonAsync<List<JsonElement>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(books);
        Assert.Equal(2, books.Count);
        Assert.Contains(books, book => book.GetProperty("title").GetString() == "Dune");
        Assert.Contains(books, book => book.GetProperty("title").GetString() == "Clean Code");
    }

    [Fact]
    public async Task GetBooks_ReturnsEmptyArrayWhenThereAreNoBooks()
    {
        var response = await _client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var books = await response.Content.ReadFromJsonAsync<List<JsonElement>>();

        Assert.NotNull(books);
        Assert.Empty(books);
    }

    [Fact]
    public async Task GetBooks_ResponseMatchesSnapshot()
    {
        var author = new Author
        {
            AuthorId = Guid.NewGuid(),
            Name = "Robert Martin",
            AuthorNumber = "A-1"
        };
        _context.Books.Add(BookFactory.MockBook("Clean Code", "BOOK-1", null, author));
        await _context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await VerifyJson(await response.Content.ReadAsStringAsync());
    }
}
