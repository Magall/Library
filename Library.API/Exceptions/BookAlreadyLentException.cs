namespace Library.Exceptions;

public class BookAlreadyLentException(string message) : InvalidOperationException(message)
{
}