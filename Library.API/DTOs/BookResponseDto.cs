using Library.DTOs;

namespace Library.DTOs;

public class BookDto
{
    public DateOnly PublishDate { get; set; }
    public String? BookNumber { get; set; }
    public List<AuthorDto> Authors { get; set; }
    public String? Title { get; set; }
}