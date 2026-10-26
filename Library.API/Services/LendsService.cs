using Library.Database.Models;
using Library.DTOs;
using Library.Exceptions;
using Library.Repositories;
using Library.Settings;
using Library.Utils;
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
    public async Task<LendResponseDto> Lend(Guid bookItemId, string SSN)
    {
        var user = await _usersRepository.GetUserBySSN(SSN);
        var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);

        UsersValidations(user, today, SSN);
        await BooksValidations(bookItemId, user.UserId);

        var lendDurationInBusinessDays = _options.LendingDurationInWorkingDays;
        var returnalDate = DateExtensions.AddWorkingDays(today, lendDurationInBusinessDays);

        var lend = await _lendsRepository.CreateLend(bookItemId, user.UserId, today, returnalDate);

        var lendDto = new LendResponseDto
        {
            BookItemId = lend.BookItemId,
            ReturnalDate = lend.EndDate,
            Name = user.Name,
            SSN = user.SSN
        };

        return lendDto;
    }

    public async Task<LendReturnResponseDto> ReturnLend(Guid bookItemId, string SSN)
    {
        var user = await _usersRepository.GetUserBySSN(SSN);
        if (user is null)
        {
            throw new UserNotFoundException($"User not found by SSN {SSN}");
        }
        var lend = await _lendsRepository.GetOpenLend(bookItemId, user.UserId);
        if (lend is null)
        {
            throw new LendNotFoundException($"This book {bookItemId} is not lent for this user {SSN}");
        }
        var returnedDate = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);

        var lendReturn = await _lendsRepository.ReturnLend(lend.BookItemId, user.UserId, returnedDate);
        var lateInterval = returnedDate.DayNumber - lend.EndDate.DayNumber;


        if (lateInterval > 0)
        {
            var endOfBan = DateExtensions.AddWorkingDays(returnedDate, (_options.BanishMultiplier * lateInterval));
            user = await _usersRepository.UpdateEndOfBan(user.UserId, endOfBan);
        }

        var result = new LendReturnResponseDto
        {
            SSN = user.SSN,
            Name = user.Name,
            ReturnedDate = lendReturn.ReturnedDate
                ?? throw new InvalidOperationException("Lend was returned but has no ReturnedDate"),
            BookItemId = lend.BookItemId,
            EndOfBan = lateInterval > 0 ? user.EndOfBan : null
        };


        return result;
    }

    private async Task BooksValidations(Guid bookItemId, Guid userId)
    {

        var booksOnUserBehold = await _lendsRepository.GetQuantityOfBooksOnUsersBehold(userId);
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

    private static void UsersValidations(User? user, DateOnly today, string SSN)
    {
        if (user is null)
        {
            throw new UserNotFoundException($"User not found by SSN {SSN}");
        }
        if (user.EndOfBan is not null && user.EndOfBan >= today)
        {
            throw new UserBannedException($"User is banned until {user.EndOfBan}");
        }
    }
}