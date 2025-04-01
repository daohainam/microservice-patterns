namespace TransactionalOutbox.Banking.AccountService.Infrastructure.Data;
public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; } = default!;
    public DbSet<Transaction> Transactions { get; set; } = default!;
}
