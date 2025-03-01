namespace CQRS.Library.BorrowerApi.Infrastructure.Entity;
public class BorrowerDbContext(DbContextOptions<BorrowerDbContext> options) : DbContext(options)
{
    public DbSet<Borrower> Borrowers { get; set; } = default!;
}
