namespace CQRS.Library.BorrowingApi.Infrastructure.Data;
public class BorrowingDbContext(DbContextOptions<BorrowingDbContext> options) : DbContext(options)
{
    public DbSet<Borrowing> Borrowings { get; set; } = default!;
}
