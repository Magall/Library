namespace Library.Repositories;
using Library.Database.Models;

public interface IUserRepository
{
    public Task<User?> GetUserBySSN(string SSN);
}