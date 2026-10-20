namespace Library.Exceptions;

public class LendNotFoundException(string message) : InvalidOperationException(message)
{
}