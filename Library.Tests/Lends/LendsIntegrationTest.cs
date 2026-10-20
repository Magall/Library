using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Library.Database;
using Library.Database.Models;
using Library.DTOs;
using Library.Tests.Book;
using Library.Tests.Lends;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Tests;

public sealed class LendsIntegrationTest : IClassFixture<ApiFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly LibraryDbContext _context;

    public LendsIntegrationTest(ApiFactory factory)
    {
        _client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        _context.Lends.ExecuteDelete();
        _context.BookItems.ExecuteDelete();
        _context.Books.ExecuteDelete();
        _context.Authors.ExecuteDelete();
        _context.Users.ExecuteDelete();

    }


    [Fact]
    public async Task LendsPost_ReturnsLendData()
    {
        var userId = Guid.NewGuid();
        var Ssn = "SSN-01";
        var bookId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        var user = LendsFactory.MockUser(userId, Ssn);
        var book = BookFactory.MockBook(bookId);
        var bookItem = LendsFactory.MockBookItem(bookId, bookItemId);
        _context.Users.Add(user);
        _context.Books.Add(book);
        _context.BookItems.Add(bookItem);
        await _context.SaveChangesAsync();
        var request = new LendRequestDto
        {
            SSN = Ssn,
            BookItemId = bookItemId
        };

        var response = await _client.PostAsJsonAsync("/api/lends", request);
        var lendDto = await response.Content.ReadFromJsonAsync<LendResponseDto>();

        Assert.NotNull(lendDto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(bookItemId, lendDto.BookItemId);
        Assert.Equal(Ssn, lendDto.SSN);
        Assert.Equal(new DateOnly(2026, 10, 19), lendDto.ReturnalDate);
    }

    [Fact]
    public async Task ReturnLends_DisplaysCorrectData()
    {
        var userId = Guid.NewGuid();
        var Ssn = "SSN-01";
        var bookId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        var user = LendsFactory.MockUser(userId, Ssn);
        var book = BookFactory.MockBook(bookId);
        var bookItem = LendsFactory.MockBookItem(bookId, bookItemId);
        var startDate = new DateOnly(2026, 10, 05);
        var endDate = new DateOnly(2026, 10, 12);
        var today = new DateOnly(2026,10,12); // Setted as fake timer on ApiFactory
        var lend = LendsFactory.MockLend(userId, bookItemId, startDate, endDate);
        _context.Users.Add(user);
        _context.Books.Add(book);
        _context.BookItems.Add(bookItem);
        _context.Lends.Add(lend);
        await _context.SaveChangesAsync();
        var request = new LendRequestDto
        {
            SSN = Ssn,
            BookItemId = bookItemId
        };

        var response = await _client.PostAsJsonAsync("/api/lends/return", request);
        var returnLendDto = await response.Content.ReadFromJsonAsync<LendReturnResponseDto>();

        Assert.NotNull(returnLendDto);
        Assert.Equal(Ssn,returnLendDto.SSN);
        Assert.Equal("My name",returnLendDto.Name);
        Assert.Equal(bookItemId,returnLendDto.BookItemId);
        Assert.Equal(today,returnLendDto.ReturnedDate);
    }
    public void Dispose()
    {
        _scope.Dispose();
    }
}
