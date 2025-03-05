namespace Saga.OnlineStore.BankCardService.Infrastructure.Data;
public class BankCardDbContext(DbContextOptions<BankCardDbContext> options) : DbContext(options)
{
    public DbSet<Card> Cards { get; set; } = default!;
}
