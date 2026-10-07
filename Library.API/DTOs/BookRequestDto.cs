namespace Library.DTOs;

public class BookRequestDto
{
    public String? Title { get; set; }
    public String? BookNumber { get; set; }
    public Guid? AuthorID{ get; set; }
}