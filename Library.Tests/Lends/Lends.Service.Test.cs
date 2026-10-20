
using NSubstitute;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Library.Settings;
using Library.Repositories;
using Library.Services;
using Library.Exceptions;
using Library.Database.Models;
using Microsoft.Extensions.Time.Testing;
using Library.Utils;
using NSubstitute.ReturnsExtensions;
using Library.Tests.Lends;
using Library.Tests.Book;

namespace Library.Tests;

public class LendsServiceTest
{

    private readonly IConfigurationSection _lendsSection;
    public LendsServiceTest()
    {
        _lendsSection = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build().GetSection(LendsOptions.SectionName);
    }


    [Fact]
    public void EnsureMaxSimultaneousLendsExistsAndHasValues()
    {
        var maxSimultaneosLends = _lendsSection
            .Get<LendsOptions>()?.MaxSimultaneousLends;

        Assert.NotNull(_lendsSection);
        Assert.True(maxSimultaneosLends > 0);
    }

    [Fact]
    public async Task LendBook_ThrowsExceptionIfUserIsNotFoundBySSN()
    {
        var lendsRepository = Substitute.For<ILendsRepository>();
        var usersRepository = Substitute.For<IUserRepository>();
        usersRepository.GetUserBySSN("SSN001").Returns((User?)null);
        var options = new LendsOptions { MaxSimultaneousLends = 3, LendingDurationInWorkingDays = 5 };
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero));
        var lendingService = new LendsService(
            usersRepository,
            lendsRepository,
            Options.Create(options),
            timeProvider
        );

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
        lendingService.Lend(Guid.NewGuid(), "SSN001"));

    }

    [Fact]
    public async Task LendBook_ThrowsToManyBooksWhenLimitIsReached()
    {
        var maxSimultaneosLends = 3;
        var userId = Guid.NewGuid();
        var bookGuid = Guid.NewGuid();
        var userSSN = "SSN001";
        var lendsRepository = Substitute.For<ILendsRepository>();
        lendsRepository.GetQuantityOfBooksOnUsersBehold(userId).Returns(maxSimultaneosLends);
        var lendsOptions = new LendsOptions { MaxSimultaneousLends = maxSimultaneosLends };
        Assert.NotNull(lendsOptions);
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero));
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserBySSN(userSSN).Returns(new User
        {
            UserId = userId,
            Name = "Rafael Gomes",
            SSN = userSSN,
            IsActive = true,

        });


        var lendsService = new LendsService(userRepository, lendsRepository, Options.Create(lendsOptions), timeProvider);

        await Assert.ThrowsAsync<TooManyBooksException>(() =>
            lendsService.Lend(bookGuid, userSSN));

    }

    [Fact]
    public async Task LendBook_ShowsExceptionWhenTryingToLendABookItemAlreadyLended()
    {
        var bookItemId = Guid.NewGuid();
        var userSSN = "SSN001";
        var lendsRepository = Substitute.For<ILendsRepository>();
        var lendsOptions = new LendsOptions { MaxSimultaneousLends = 3 };
        lendsRepository.GetQuantityOfBooksOnUsersBehold(Guid.NewGuid()).Returns(1);
        lendsRepository.IsBookItemLent(bookItemId).Returns(true);
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero));
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserBySSN(userSSN).Returns(new User
        {
            UserId = Guid.NewGuid(),
            Name = "Rafael Gomes",
            SSN = userSSN,
            IsActive = true,

        });


        var lendsService = new LendsService(userRepository, lendsRepository, Options.Create(lendsOptions), timeProvider);

        await Assert.ThrowsAsync<BookAlreadyLentException>(() =>
            lendsService.Lend(bookItemId, userSSN));

    }

    [Fact]
    public async Task LendBook_LendsBookIfAllValidationsPasses()
    {
        var bookItemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var userSSN = "SSN001";
        var today = new DateOnly(2026, 10, 12);
        var options = new LendsOptions
        {
            MaxSimultaneousLends = 1,
            LendingDurationInWorkingDays = 5
        };
        var lendsRepository = Substitute.For<ILendsRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        lendsRepository.IsBookItemLent(bookItemId).Returns(false);
        userRepository.GetUserBySSN(userSSN).Returns(new User
        {
            UserId = userId,
            Name = "Rafael Gomes",
            SSN = userSSN,
            IsActive = true,

        });
        var timeProvider = new FakeTimeProvider(
                new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero)
        );
        var returnalDate = DateExtensions.AddWorkingDays(today, options.LendingDurationInWorkingDays);
        lendsRepository.CreateLend(bookItemId, userId, today, returnalDate).Returns(new Lend
        {
            LendId = Guid.NewGuid(),
            BookItemId = bookItemId,
            UserId = userId,
            StartDate = new DateOnly(2026, 10, 12),
            EndDate = new DateOnly(2026, 10, 19),

        });

        var lendsService = new LendsService(
            userRepository,
            lendsRepository,
            Options.Create(options),
            timeProvider
        );

        var response = await lendsService.Lend(bookItemId, userSSN);

        Assert.Equal(bookItemId, response.BookItemId);
        Assert.Equal("SSN001", response.SSN);
        Assert.Equal(new DateOnly(2026, 10, 19), response.ReturnalDate);
    }

    [Fact]
    public async Task ReturnLend_ThrowsUserNotFoundExceptionIfSSNnotFound()
    {
        var bookItemId = Guid.NewGuid();
        var SSN = "SSN-001";
        var lendsRepository = Substitute.For<ILendsRepository>();
        var usersRepository = Substitute.For<IUserRepository>();
        var options = new LendsOptions { };
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero));
        usersRepository.GetUserBySSN(SSN).ReturnsNull();
        var lendsService = new LendsService(usersRepository, lendsRepository, Options.Create(options), timeProvider);

        await Assert.ThrowsAsync<UserNotFoundException>(() => lendsService.ReturnLend(bookItemId, SSN));
    }

    [Fact]
    public async Task ReturnLend_ThrowsBookIsNotLentForUserIfLendNotFound()
    {
        var bookItemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var SSN = "SSN-001";
        var user = LendsFactory.MockUser(userId, SSN);
        var usersRepository = Substitute.For<IUserRepository>();
        var lendsRepository = Substitute.For<ILendsRepository>();
        var options = new LendsOptions { };
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero));
        usersRepository.GetUserBySSN(SSN).Returns(user);
        lendsRepository.GetOpenLend(bookItemId, userId).ReturnsNull();

        var lendsService = new LendsService(usersRepository, lendsRepository, Options.Create(options), timeProvider);
        await Assert.ThrowsAsync<LendNotFoundException>(() => lendsService.ReturnLend(bookItemId, SSN));
    }

    [Fact]
    public async Task ReturnLend_ReturnsDtoOnSuccessfullReturn()
    {
        var timeProvider = new FakeTimeProvider
            (new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero));
        var userId = Guid.NewGuid();
        var bookItemId = Guid.NewGuid();
        var startDate = new DateOnly(2026, 10, 5);
        var endDate = new DateOnly(2026, 10, 12);
        var returnedDate = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var SSN = "SSN-001";
        var usersName = "Rafael";
        var lendsRepository = Substitute.For<ILendsRepository>();
        var usersRepository = Substitute.For<IUserRepository>();
        var options = new LendsOptions { };
        usersRepository.GetUserBySSN(SSN).Returns(new User
        {
            UserId = userId,
            SSN = SSN,
            Name = usersName
        });
        lendsRepository.GetOpenLend(bookItemId, userId)
            .Returns(LendsFactory.MockLend(userId, bookItemId, startDate, endDate));

        lendsRepository.ReturnLend(bookItemId, userId, returnedDate)
            .Returns(new Lend
            {
                UserId = userId,
                BookItemId = bookItemId,
                StartDate = startDate,
                EndDate = endDate,
                ReturnedDate = returnedDate
            });
        var lendsService = new LendsService(usersRepository, lendsRepository, Options.Create(options), timeProvider);


        var result = await lendsService.ReturnLend(bookItemId, SSN);


        Assert.Equal(usersName, result.Name);
        Assert.Equal(SSN, result.SSN);
        Assert.Equal(returnedDate, result.ReturnedDate);
        Assert.Equal(bookItemId, result.BookItemId);
    }



}