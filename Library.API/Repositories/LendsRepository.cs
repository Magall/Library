using Library.Database;
using Library.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Repositories;

public class LendsRepository : ILendsRepository
{

    private readonly LibraryDbContext _context;

    public LendsRepository(LibraryDbContext context)
    {
        _context = context;
    }
    public async Task<Lend> CreateLend(Guid bookItemId, Guid userId, DateOnly lendDate, DateOnly returnalDate)
    {
        var lend = new Lend
        {
            UserId = userId,
            StartDate = lendDate,
            EndDate = returnalDate,
            BookItemId = bookItemId,
        };

        _context.Lends.Add(lend);
        await _context.SaveChangesAsync();
        return lend;
    }

    public async Task<int> GetQuantityOfBooksOnUsersBehold(Guid userId)
    {
        return await _context.Lends.CountAsync(lend =>
            lend.UserId == userId && lend.ReturnedDate == null);
    }

    public async Task<bool> IsBookItemLent(Guid bookItemId)
    {
        return await _context.Lends.AnyAsync(lend =>
            lend.BookItemId == bookItemId && lend.ReturnedDate == null);
    }

    public Task<Lend> ReturnLend(Guid bookItem, Guid userId, DateOnly returnedDate)
    {
        throw new NotImplementedException();
    }

    public async Task<Lend?> GetLend(Guid bookItemId, Guid userId)
    {
        return await _context.Lends.SingleOrDefaultAsync(lend =>
            lend.BookItemId == bookItemId && lend.UserId == userId && lend.ReturnedDate == null);
    }
}