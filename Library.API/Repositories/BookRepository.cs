using Library.Database;
using Library.Database.Models;
using Library.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Library.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;
    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetBooks(BookRequestDto bookRequest)
    {
        var books = _context.Books.AsQueryable();

        if (bookRequest.AuthorID.HasValue)
        {
            books = books.Where(
                    book => book.Authors.
                        Any(author => author.AuthorId == bookRequest.AuthorID));
        }

        if (!string.IsNullOrEmpty(bookRequest.BookNumber))
        {
            books = books.Where(book => book.BookNumber.Contains(bookRequest.BookNumber));
        }

        if (!string.IsNullOrEmpty(bookRequest.Title))
        {
            books = books.Where(book => book.Title.Contains(bookRequest.Title));
        }

        return await books
            .Include(book => book.Authors)
            .ToListAsync();
    }
}