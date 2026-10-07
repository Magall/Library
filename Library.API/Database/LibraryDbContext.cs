using Library.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Database;
public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookItem> BookItems { get; set; }
    public DbSet<Lend> Lends { get; set; }
    public DbSet<User> Users { get; set; }
}