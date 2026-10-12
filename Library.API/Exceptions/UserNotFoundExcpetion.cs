namespace Library.Exceptions;

public class UserNotFoundException : InvalidOperationException
{
    public UserNotFoundException(string message) : base(message)
    {

    }
}