using Library.Database;
using Library.Database.Models;
using Library.Repositories;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly LibraryDbContext _context;

    public UserRepository(LibraryDbContext context)
    {
        _context = context;
    }
    public async Task<User?> GetUserBySSN(string SSN)
    {
        return await _context.Users.SingleOrDefaultAsync(user => user.SSN == SSN);
    }

    public async Task<User> UpdateEndOfBan(Guid userId, DateOnly endOfBan)
    {
        var user = await _context.Users.SingleAsync(user => user.UserId == userId);
        user.EndOfBan = endOfBan;
        await _context.SaveChangesAsync();
        return user;
    }
}