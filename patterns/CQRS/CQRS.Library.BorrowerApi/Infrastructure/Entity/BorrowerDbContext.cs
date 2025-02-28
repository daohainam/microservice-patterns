namespace CQRS.Library.BorrowerApi.Infrastructure.Entity;
public class BorrowerDbContext: DbContext
{
    public BorrowerDbContext(DbContextOptions<BorrowerDbContext> options) : base(options)
    {
    }
    public DbSet<Borrower> Borrowers { get; set; } = default!;
}
