using Library.Database.Models;

namespace Library.Repositories;

public interface ILendsRepository
{
    public Task<int> GetQuantityOfBooksOnUsersBehold(Guid userId);
    public Task<bool> IsBookItemLent(Guid bookItemId);
    public Task<Lend> CreateLend(Guid bookItemId, Guid userId, DateOnly lendDate, DateOnly returnalDate);
    public Task<Lend?> GetOpenLend(Guid bookItemId, Guid userId);
    public Task<Lend> ReturnLend(Guid bookItem, Guid userId, DateOnly returnedDate);
}