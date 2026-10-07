using Library.DTOs;
namespace Library.Services;

public interface IBookService
{
   public Task<IEnumerable<BookDto>> GetBooks(BookRequestDto bookRequest);
}