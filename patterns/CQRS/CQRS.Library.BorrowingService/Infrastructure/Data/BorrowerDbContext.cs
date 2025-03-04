using CQRS.Library.BorrowingService.Infrastructure.Entity;

namespace CQRS.Library.BorrowingService.Infrastructure.Data;
public class BorrowingDbContext(DbContextOptions<BorrowingDbContext> options) : DbContext(options)
{
    public DbSet<Borrowing> Borrowings { get; set; } = default!;
}
