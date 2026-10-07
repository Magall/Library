using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace Library.Database.Models;

[Table("Books")]
public class Book
{
    [Key]
    public Guid BookId { get; set; }

    [Required]
    public DateOnly PublishDate { get; set; }

    [Required]
    public String? BookNumber { get; set; }

    public List<Author> Authors { get; set;}

    [Required]
    public String? Title { get; set; }

    public ICollection<BookItem> BookItems { get; } = new List<BookItem>();
}