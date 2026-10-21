namespace Library.DTOs;

public class LendReturnResponseDto
{
    public string SSN { get; set; }
    public string Name { get; set; }
    public DateOnly ReturnedDate { get; set; }
    public Guid BookItemId { get; set; }
    public DateOnly? EndOfBan { get; set; }
}