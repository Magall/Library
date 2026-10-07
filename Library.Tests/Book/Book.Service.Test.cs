using Library.DTOs;
using Library.Repositories;
using NSubstitute;
using Mapster;
using MapsterMapper;
using Library.Services;

namespace Library.Tests.Book;

public class BookServiceTest
{
    [Fact]
    public async Task GetBooks_MapsRepositoryBooksToDtos()
    {
        var request = new BookRequestDto
        {
            Title = "Clea"
        };

        var book = BookFactory.MockBook("Clean Code", BookFactory.Author("Robert Martin"));
        var mapper = new Mapper(TypeAdapterConfig.GlobalSettings);
        var repositoryMock = Substitute.For<IBookRepository>();
        repositoryMock.GetBooks(request).Returns([book]);
        var service = new BookService(repositoryMock, mapper);
        
        var serviceResponse = await service.GetBooks(request);

        var dto = Assert.Single(serviceResponse);
        Assert.Equal("Clean Code", dto.Title);
        Assert.Equal(book.BookNumber, dto.BookNumber);
        Assert.Equal(book.PublishDate, dto.PublishDate);
        Assert.Equal("Robert Martin", Assert.Single(dto.Authors).Name);

    }
}