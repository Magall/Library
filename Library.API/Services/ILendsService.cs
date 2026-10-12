using Library.Database.Models;
namespace Library.Services;

public interface ILendsService
{
    public Task<LendDto> LendBook(Guid bookItemId, string SSN);
}