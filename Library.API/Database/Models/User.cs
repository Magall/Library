using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Database.Models;

[Table("Users")]
public class User
{
    [Key]
    public Guid UserId { get; set; }

    [Required]
    public String? Name { get; set; }

    [Required]
    public String? SSN { get; set; }

    public DateOnly? EndOfBan { get; set; }
    
    [Required]
    public bool IsActive { get; set; }

    public ICollection<Lend> Lends { get; } = new List<Lend>();
}