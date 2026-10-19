using Library.Database.Models;

namespace Library.Repositories;

public interface ILendsRepository
{
    public Task<int> GetQuantityOfBooksOnUsersBehold(Guid userId);
    public Task<bool> IsBookItemLent(Guid bookItemId);
    public Task<Lend> CreateLend(Guid bookItemId, Guid userId, DateOnly lendDate, DateOnly returnalDate);
}