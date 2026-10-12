using Library.Database.Models;
using Library.Exceptions;
using Library.Repositories;
using Library.Settings;
using Microsoft.Extensions.Options;

namespace Library.Services;

public class LendsService : ILendsService
{
    private readonly IUserRepository _usersRepository;
    private readonly ILendsRepository _lendsRepository;
    private readonly LendsOptions _options;
    private readonly TimeProvider _timeProvider;

    public LendsService(IUserRepository userRepository, ILendsRepository lendsRepository, IOptions<LendsOptions> options, TimeProvider timeProvider)
    {
        _usersRepository = userRepository;
        _lendsRepository = lendsRepository;
        _options = options.Value;
        _timeProvider = timeProvider;
    }
    public async Task<LendDto> LendBook(Guid bookItemId, string SSN)
    {
        var user = await _usersRepository.GetUserBySSN(SSN);
        if (user is null)
        {
            throw new UserNotFoundException($"User not found by SSN {SSN}");
        }
        await BooksValidations(bookItemId, SSN);

        var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        var lend = await _lendsRepository.CreateLend(bookItemId, user.UserId, today);

        var lendDto = new LendDto
        {
            BookItemId = lend.BookItemId,
            ReturnalDate = lend.EndDate,
            Name = user.Name,
            SSN = user.SSN
        };

        return lendDto;
    }
    private async Task BooksValidations(Guid bookItemId, string SSN)
    {

        var booksOnUserBehold = await _lendsRepository.GetQuantityOfBooksOnUsersBehold(SSN);
        if (booksOnUserBehold >= _options.MaxSimultaneousLends)
        {
            throw new TooManyBooksException(
                $"User already holds limits of simoultaneous lends {_options.MaxSimultaneousLends}");
        }
        var isBookItemLent = await _lendsRepository.IsBookItemLent(bookItemId);
        if (isBookItemLent)
        {
            throw new BookAlreadyLentException($"This book is already lent {bookItemId}");
        }
    }
}