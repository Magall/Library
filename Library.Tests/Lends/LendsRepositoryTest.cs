using Library.Database;
using Library.Database.Models;
using Library.Repositories;
using Library.Tests.Book;
using Library.Tests.Lends;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace Library.Tests;

public class LendsRepositoryTest : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LibraryDbContext _context;
    private readonly LendsRepository _repository;
    public LendsRepositoryTest()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
        .UseSqlite(_connection)
        .Options;

        _context = new LibraryDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new LendsRepository(_context);
    }

    [Fact]
    public async Task IsBookItemLent_ReturnsTrueWhenBookItemIsLent()
    {
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();

        var lend = new Lend
        {
            UserId = userId,
            LendId = Guid.NewGuid(),
            BookItemId = bookItemId,
            StartDate = new DateOnly(2026, 10, 5),
            EndDate = new DateOnly(2026, 10, 12)
        };

        await CreateLends([lend], bookId, userId, bookItemId);

        var result = await _repository.IsBookItemLent(bookItemId);

        Assert.True(result);
    }
    [Fact]
    public async Task IsBookLent_ReturnsFalseIfBookIsReturned()
    {
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        var lend = new Lend
        {
            UserId = userId,
            LendId = Guid.NewGuid(),
            BookItemId = bookItemId,
            StartDate = new DateOnly(2026, 10, 5),
            EndDate = new DateOnly(2026, 10, 12),
            ReturnedDate = new DateOnly(2025, 10, 6)
        };
        await CreateLends([lend], bookId, userId, bookItemId);


        var result = await _repository.IsBookItemLent(bookItemId);

        Assert.False(result);
    }
    [Fact]
    public async Task IsBookLent_ReturnsFalseIfLendIsForOtherItem()
    {
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();

        var lend = new Lend
        {
            LendId = Guid.NewGuid(),
            UserId = userId,
            BookItemId = bookItemId,
            StartDate = new DateOnly(2026, 10, 5),
            EndDate = new DateOnly(2026, 10, 12)
        };

        await CreateLends([lend], bookId, userId, bookItemId);
        var result = await _repository.IsBookItemLent(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task IsBookLent_ReturnsTrueForBookLentWithHistory()
    {
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();

        var returnedLend = new Lend
        {
            LendId = Guid.NewGuid(),
            UserId = userId,
            BookItemId = bookItemId,
            StartDate = new DateOnly(2026, 10, 5),
            EndDate = new DateOnly(2026, 10, 12),
            ReturnedDate = new DateOnly(2026, 10, 12)
        };
        var currentLend = new Lend
        {
            LendId = Guid.NewGuid(),
            UserId = userId,
            BookItemId = bookItemId,
            StartDate = new DateOnly(2026, 10, 13),
            EndDate = new DateOnly(2026, 10, 21),
        };
        await CreateLends([returnedLend, currentLend], bookId, userId, bookItemId);

        var result = await _repository.IsBookItemLent(bookItemId);

        Assert.True(result);

    }

    [Fact]
    public async Task IsBookLent_RetursFalseIfDbIsEmpty()
    {
        var result = await _repository.IsBookItemLent(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task GetQuantityOfBooksOnUserBehold_ReturnsCorrectQuantity()
    {
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        var lend = new Lend
        {
            LendId = Guid.NewGuid(),
            UserId = userId,
            BookItemId = bookItemId,
            StartDate = new DateOnly(2026, 10, 5),
            EndDate = new DateOnly(2026, 10, 12)
        };
        await CreateLends([lend], bookId, userId, bookItemId);

        var result = await _repository.GetQuantityOfBooksOnUsersBehold(userId);

        Assert.Equal(1, result);
    }
    [Fact]
    public async Task GetQuantityOfBooksOnUsersBehold_OnlyCountsActiveLends()
    {
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();

        var returnedLend = new Lend
        {
            LendId = Guid.NewGuid(),
            UserId = userId,
            BookItemId = bookItemId,
            StartDate = new DateOnly(2026, 10, 5),
            EndDate = new DateOnly(2026, 10, 12),
            ReturnedDate = new DateOnly(2026, 10, 12)
        };
        var currentLend = new Lend
        {
            LendId = Guid.NewGuid(),
            UserId = userId,
            BookItemId = bookItemId,
            StartDate = new DateOnly(2026, 10, 13),
            EndDate = new DateOnly(2026, 10, 21),
        };
        await CreateLends([returnedLend, currentLend], bookId, userId, bookItemId);

        var result = await _repository.GetQuantityOfBooksOnUsersBehold(userId);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task CreateLend_SavesLendForGivenUserBookItemAndStartdate()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        await CreateUser(userId);
        await CreateBookItem(bookId, bookItemId);
        var lendDate = new DateOnly(2026, 10, 13);
        var returnalDate = new DateOnly(2026, 10, 20);

        await _repository.CreateLend(bookItemId, userId, lendDate, returnalDate);

        var lend = await _context.Lends.AsNoTracking().SingleOrDefaultAsync(lend =>
            lend.BookItemId == bookItemId
            && lend.UserId == userId
            && lend.StartDate == lendDate
        );

        Assert.NotNull(lend);
    }

    [Fact]
    public async Task CreateLend_ReturnsInsertedLend()
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        await CreateUser(userId);
        await CreateBookItem(bookId, bookItemId);
        var lendDate = new DateOnly(2026, 10, 13);
        var returnalDate = new DateOnly(2026, 10, 20);

        var result = await _repository.CreateLend(bookItemId, userId, lendDate, returnalDate);

        Assert.Equal(userId, result.UserId);
        Assert.Equal(bookItemId, result.BookItemId);
        Assert.Equal(returnalDate, result.EndDate);
        Assert.Equal(lendDate, result.StartDate);
        Assert.Null(result.ReturnedDate);
    }

    [Fact]
    public async Task GetLends_ReturnsOpenLendForUserAndBookId()
    {
        var userId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 10, 12);
        var endDate = new DateOnly(2026, 10, 19);
        await CreateLends([LendsFactory.MockLend(userId, bookItemId, startDate, endDate)],
            bookId,
            userId,
            bookItemId
        );

        var lendsRepository = new LendsRepository(_context);
        var lend = await lendsRepository.GetOpenLend(bookItemId, userId);

        Assert.NotNull(lend);
        Assert.Equal(bookItemId, lend.BookItemId);
        Assert.Equal(userId, lend.UserId);
        Assert.Null(lend.ReturnedDate);
    }
    [Fact]
    public async Task GetLends_ReturnsNullIfLendsHaveBeenReturned()
    {
        var userId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 10, 12);
        var endDate = new DateOnly(2026, 10, 19);
        var returnedDate = new DateOnly(2026, 10, 19);
        await CreateLends([LendsFactory.MockLend(userId, bookItemId, startDate, endDate, returnedDate)],
            bookId,
            userId,
            bookItemId
        );

        var lendsRepository = new LendsRepository(_context);
        var result = await lendsRepository.GetOpenLend(bookItemId, userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLends_ReturnsNullIfLendBelongsToOtherUser()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 10, 12);
        var endDate = new DateOnly(2026, 10, 19);
        await CreateLends([LendsFactory.MockLend(userId, bookItemId, startDate, endDate)],
            bookId,
            userId,
            bookItemId
        );

        var lendsRepository = new LendsRepository(_context);
        var result = await lendsRepository.GetOpenLend(bookItemId, otherUserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLends_ReturnsNullIfLendBelongsToOtherBookItem()
    {
        var userId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        var otherBookItemId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 10, 12);
        var endDate = new DateOnly(2026, 10, 19);
        await CreateLends([LendsFactory.MockLend(userId, bookItemId, startDate, endDate)],
            bookId,
            userId,
            bookItemId
        );

        var lendsRepository = new LendsRepository(_context);
        var result = await lendsRepository.GetOpenLend(otherBookItemId, userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLends_ReturnsOnlyOpenLend()
    {
        var userId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 10, 12);
        var endDate = new DateOnly(2026, 10, 19);
        var historyReturnedDate = new DateOnly(2026, 9, 11);
        var historyStartDate = new DateOnly(2026, 9, 5);
        var historyEndDate = new DateOnly(2026, 9, 11);
        await CreateLends([
                LendsFactory.MockLend(userId, bookItemId, startDate, endDate),
                LendsFactory.MockLend(userId, bookItemId, historyStartDate, historyEndDate, historyReturnedDate)
            ],
            bookId,
            userId,
            bookItemId
        );

        var lendsRepository = new LendsRepository(_context);
        var lend = await lendsRepository.GetOpenLend(bookItemId, userId);

        Assert.NotNull(lend);
        Assert.Equal(bookItemId, lend.BookItemId);
        Assert.Equal(userId, lend.UserId);
        Assert.Equal(startDate,lend.StartDate);
        Assert.Null(lend.ReturnedDate);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task CreateUser(Guid userId)
    {
        var user = LendsFactory.MockUser(userId);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();
    }

    private async Task CreateBookItem(Guid bookId, Guid bookItemId)
    {
        var book = BookFactory.MockBook(bookId);
        var bookItem = LendsFactory.MockBookItem(bookId, bookItemId);

        _context.Books.Add(book);
        _context.BookItems.Add(bookItem);

        await _context.SaveChangesAsync();

    }

    private async Task CreateLends(Lend[] lends, Guid bookId, Guid userId, Guid bookItemId)
    {
        await CreateUser(userId);
        await CreateBookItem(bookId, bookItemId);


        _context.Lends.AddRange(lends);
        await _context.SaveChangesAsync();
    }

}