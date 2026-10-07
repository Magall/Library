using Library.Database.Models;
using Library.DTOs;

namespace Library.Repositories;

public interface IBookRepository
{
    public Task<IEnumerable<Book>> GetBooks(BookRequestDto bookRequest);
}