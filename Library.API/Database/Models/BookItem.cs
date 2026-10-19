using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Database.Models;

[Table("BookItem")]
public class BookItem
{
    [Key]
    public Guid BookItemId { get; set; }

    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
    public ICollection<Lend> Lends { get; set; }
}