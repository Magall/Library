using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Database.Models
{
    [Table("Lends")]
    public class Lend
    {
        [Key]
        public Guid LendId { get; set; }

        public Guid BookItemId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly EndDate { get; set; }

        public DateOnly? ReturnedDate { get; set; }

        public BookItem BookItem { get; set; } = null!;
        public User User { get; set; } = null!;

    }
}