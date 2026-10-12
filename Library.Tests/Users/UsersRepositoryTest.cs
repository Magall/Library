using Library.Database;
using Library.Database.Models;
using Library.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Library.Tests;

public class UserRepositoryTest : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LibraryDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTest()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LibraryDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new UserRepository(_context);

    }

    public void Dispose()
    {
        _connection.Dispose();
        _context.Dispose();
    }

    [Fact]
    public async Task GetUserBySSN_ReturnsNullWhenThereIsNoUser()
    {
        var result = await _repository.GetUserBySSN("SSN01");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserBySSN_ReturnsUser()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Name = "Rafael Gomes",
            SSN = "SSN01",
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserBySSN("SSN01");
        Assert.NotNull(result);
        Assert.Equal("Rafael Gomes", result.Name);
        Assert.Equal(userId, result.UserId);
    }


}