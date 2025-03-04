using CQRS.Library.BorrowerService.Infrastructure.Entity;

namespace CQRS.Library.BorrowerService.Infrastructure.Data;
public class BorrowerDbContext(DbContextOptions<BorrowerDbContext> options) : DbContext(options)
{
    public DbSet<Borrower> Borrowers { get; set; } = default!;
}
