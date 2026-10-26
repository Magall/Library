namespace Library.Exceptions;

public class UserBannedException : InvalidOperationException
{
    public UserBannedException(string message) : base(message)
    {

    }
}