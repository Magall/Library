// If user has reached max simultaneous lend we shoud show an exception

using Castle.Core.Configuration;
using Microsoft.VisualBasic;
using NSubstitute;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Library.Settings;
using Library.Repositories;
using Library.Services;
using Microsoft.EntityFrameworkCore.Query;
using Library.Exceptions;
using Library.Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Time.Testing;
using NSubstitute.ReturnsExtensions;

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
        lendingService.LendBook(Guid.NewGuid(), "SSN001"));

    }

    [Fact]
    public async Task LendBook_ThrowsToManyBooksWhenLimitIsReached()
    {
        var maxSimultaneosLends = 3;
        var bookGuid = Guid.NewGuid();
        var userSSN = "SSN001";
        var lendsRepository = Substitute.For<ILendsRepository>();
        lendsRepository.GetQuantityOfBooksOnUsersBehold("SSN001").Returns(maxSimultaneosLends);
        var lendsOptions = new LendsOptions { MaxSimultaneousLends = maxSimultaneosLends };
        Assert.NotNull(lendsOptions);
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero));
        var userRepository = Substitute.For<IUserRepository>();


        var lendsService = new LendsService(userRepository, lendsRepository, Options.Create(lendsOptions), timeProvider);

        await Assert.ThrowsAsync<TooManyBooksException>(() =>
            lendsService.LendBook(bookGuid, userSSN));

    }

    [Fact]
    public async Task LendBook_ShowsExceptionWhenTryingToLendABookItemAlreadyLended()
    {
        var bookItemId = Guid.NewGuid();
        var userSSN = "SSN001";
        var lendsRepository = Substitute.For<ILendsRepository>();
        var lendsOptions = new LendsOptions { MaxSimultaneousLends = 3 };
        lendsRepository.GetQuantityOfBooksOnUsersBehold(userSSN).Returns(1);
        lendsRepository.IsBookItemLent(bookItemId).Returns(true);
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero));
        var userRepository = Substitute.For<IUserRepository>();


        var lendsService = new LendsService(userRepository, lendsRepository, Options.Create(lendsOptions), timeProvider);

        await Assert.ThrowsAsync<BookAlreadyLentException>(() =>
            lendsService.LendBook(bookItemId, userSSN));

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
        lendsRepository.CreateLend(bookItemId, userId, today).Returns(new Lend
        {
            LendId = Guid.NewGuid(),
            BookItemId = bookItemId,
            UserId = userId,
            StartDate = new DateOnly(2026, 10, 12),
            EndDate = new DateOnly(2026, 10, 19),

        });

        var timeProvider = new FakeTimeProvider(
                new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero)
        );
        var lendsService = new LendsService(
            userRepository,
            lendsRepository,
            Options.Create(options),
            timeProvider
        );

        var response = await lendsService.LendBook(bookItemId, userSSN);


        Assert.Equal(bookItemId, response.BookItemId);
        Assert.Equal("SSN001", response.SSN);
        Assert.Equal(new DateOnly(2026, 10, 19), response.ReturnalDate);
    }


}