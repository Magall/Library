using Library.DTOs;
using Library.Repositories;
using Mapster;
using MapsterMapper;

namespace Library.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public BookService(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BookDto>> GetBooks(BookRequestDto bookRequest)
    {
        var booksFromDB =  await _bookRepository.GetBooks(bookRequest);
        var booksResponseDTO = _mapper.Map<IEnumerable<BookDto>>(booksFromDB);

        return booksResponseDTO;
    }
}