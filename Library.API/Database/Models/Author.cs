using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Library.Database.Models;

[Table("Authors")]
public class Author
{
    [Key]
    public Guid AuthorId { get; set; }
   
    [Required]
    public String? Name { get; set; }
   
    [Required]
    public String? AuthorNumber { get; set; }
   
    public List<Book> Books { get; } = [];

}