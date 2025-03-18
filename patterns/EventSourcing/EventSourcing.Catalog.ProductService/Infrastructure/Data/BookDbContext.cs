using EventSourcing.Catalog.ProductService.Infrastructure.Entity;

namespace EventSourcing.Catalog.ProductService.Infrastructure.Data;
public class BookDbContext(DbContextOptions<BookDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = default!;
}
