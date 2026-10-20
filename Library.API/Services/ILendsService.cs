using Library.Database.Models;
using Library.DTOs;
namespace Library.Services;

public interface ILendsService
{
    public Task<LendResponseDto> Lend(Guid bookItemId, string SSN);
    public Task<LendReturnResponseDto> ReturnLend(Guid bookItemId, string SSN);

}