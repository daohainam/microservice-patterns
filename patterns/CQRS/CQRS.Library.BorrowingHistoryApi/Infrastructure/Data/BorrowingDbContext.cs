namespace CQRS.Library.BorrowingHistoryApi.Infrastructure.Data;
public class BorrowingDbContext(DbContextOptions<BorrowingDbContext> options) : DbContext(options)
{
    public DbSet<BorrowingHistoryItem> BorrowingHistoryItems { get; set; } = default!;
    public DbSet<Book> Books { get; set; } = default!;
    public DbSet<Borrower> Borrowers { get; set; } = default!;
}
