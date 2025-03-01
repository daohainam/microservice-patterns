namespace CQRS.Library.BookApi.Infrastructure.Data;
public class BookDbContext(DbContextOptions<BookDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = default!;
}
