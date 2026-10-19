using Library.DTOs;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
namespace Library.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks([FromQuery]BookRequestDto requestDto)
    {
        var books = await _bookService.GetBooks(requestDto);
        return Ok(books);
    }
}