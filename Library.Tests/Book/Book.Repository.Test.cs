using Library.Database;
using Library.DTOs;
using Library.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using BookModel = Library.Database.Models.Book;

namespace Library.Tests.Book;

public sealed class BookRepositoryTest : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LibraryDbContext _context;
    private readonly BookRepository _repository;
    private static readonly Guid MartinGuid = new("c3385c46-cdca-4fac-b0fa-8b0ec54731b4");
    private static readonly Guid HerbertGuid = new("3133e094-6e6a-4686-99c5-5bc196b77d7e");


    public BookRepositoryTest()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LibraryDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new BookRepository(_context);
    }

    [Fact]
    public async Task GetBooks_ReturnsEveryRecordWhenFiltersAreEmpty()
    {
        _context.Books.AddRange(
            BookFactory.MockBook("Clean Code"),
            BookFactory.MockBook("Clean Architecture")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetBooks(new BookRequestDto());

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetBooks_ReturnsRecordsThatContainsTitle()
    {
        _context.Books.AddRange(
            BookFactory.MockBook("Clean Code"),
            BookFactory.MockBook("Clean Architecture"),
            BookFactory.MockBook("Dune")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetBooks(new BookRequestDto { Title = "Clean" });

        Assert.Equal(2, result.Count());
        Assert.Contains(result, book => book.Title == "Clean Code");
        Assert.Contains(result, book => book.Title == "Clean Architecture");
    }

    [Fact]
    public async Task GetBooks_ReturnsRecordsThatContainsBookNumber()
    {
        var request = new BookRequestDto { BookNumber = "001" };
        _context.Books.AddRange(
            BookFactory.MockBook("Clean", "BOOK-001"),
            BookFactory.MockBook("Dune", "BOOK-002")
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetBooks(request);

        var resultDto = Assert.Single(result);
        Assert.Equal("Clean", resultDto.Title);
    }

    [Fact]
    public async Task GetBooks_ReturnsRecordsThatMatchesAuthorID()
    {
        var authorOne = BookFactory.Author("Martin", MartinGuid);
        var authorTwo = BookFactory.Author("Herbert", HerbertGuid);
        _context.Books.AddRange(
            BookFactory.MockBook("Clean Architecture", authorOne),
            BookFactory.MockBook("Dune", authorTwo)
        );
        await _context.SaveChangesAsync();

        var request = new BookRequestDto { AuthorID = HerbertGuid };

        var result = await _repository.GetBooks(request);

        var dto = Assert.Single(result);
        Assert.Equal(HerbertGuid, dto.Authors[0].AuthorId);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}