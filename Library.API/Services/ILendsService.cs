using Library.Database.Models;
namespace Library.Services;

public interface ILendsService
{
    public Task<LendResponseDto> LendBook(Guid bookItemId, string SSN);
}