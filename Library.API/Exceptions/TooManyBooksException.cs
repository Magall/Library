namespace Library.Exceptions;

public class TooManyBooksException : InvalidOperationException
{
    public TooManyBooksException(string message) : base(message)
    {

    }
}