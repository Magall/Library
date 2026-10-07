using BookModel = Library.Database.Models.Book;
using Library.Database.Models;

namespace Library.Tests.Book;

public static class BookFactory
{
    public static Author Author(string name = "My author", Guid? authorId = null)
    {
        return new()
        {
            AuthorId = authorId ?? Guid.NewGuid(),
            Name = name,
            AuthorNumber = "a-" + Guid.NewGuid()
        };
    }



    public static BookModel MockBook(string title = "My Title", params Author[] authors) =>
        MockBook(title, null, authors);

    public static BookModel MockBook(string title, string? bookNumber, params Author[] authors) => new()
    {
        BookId = Guid.NewGuid(),
        Title = title,
        PublishDate = new DateOnly(2020, 8, 8),
        BookNumber = bookNumber ?? String.Format("BOOK-{0}", Guid.NewGuid()),
        Authors = authors.Length > 0 ? [.. authors] : [Author()]
    };
}