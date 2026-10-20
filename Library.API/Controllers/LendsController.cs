using Library.DTOs;
using Library.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LendsController : ControllerBase
{
    private readonly ILendsService _lendsService;

    public LendsController(ILendsService lendsService)
    {
        _lendsService = lendsService;
    }

    [HttpPost]
    public async Task<ActionResult<LendResponseDto>> CreateLend([FromBody] LendRequestDto request)
    {
        var lend = await _lendsService.Lend(request.BookItemId, request.SSN);
        return Ok(lend);
    }

    [HttpPost("return")]
    public async Task<ActionResult<LendReturnResponseDto>> ReturnLend([FromBody] LendRequestDto request)
    {
        var lend = await _lendsService.ReturnLend(request.BookItemId,request.SSN);
        return Ok(lend);    
    }
}