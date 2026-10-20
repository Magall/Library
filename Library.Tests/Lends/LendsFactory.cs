using Library.Database.Models;
using Library.Tests.Book;

namespace Library.Tests.Lends;

public static class LendsFactory
{

    public static User MockUser(Guid userId, string SSN = "SSN01")
    {
        return new User
        {
            UserId = userId,
            Name = "My name",
            SSN = SSN,
            IsActive = true
        };

    }

    public static BookItem MockBookItem(Guid bookId, Guid bookItemId)
    {

        return new BookItem
        {
            BookItemId = bookItemId,
            BookId = bookId
        };

    }

    public static Lend MockLend(Guid userId, Guid bookItemId, DateOnly startDate, DateOnly endDate, DateOnly? returnedDate = null)
    {
        return new Lend
        {
            UserId = userId,
            BookItemId = bookItemId,
            StartDate = startDate,
            EndDate = endDate,
            ReturnedDate = returnedDate
        };
    }
}

