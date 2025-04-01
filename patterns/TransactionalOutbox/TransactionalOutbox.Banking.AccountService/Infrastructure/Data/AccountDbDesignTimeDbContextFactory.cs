using Microsoft.EntityFrameworkCore.Design;

namespace TransactionalOutbox.Banking.AccountService.Infrastructure.Data
{
    public class AccountDbDesignTimeDbContextFactory: IDesignTimeDbContextFactory<AccountDbContext>
    {
        public AccountDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AccountDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=account;Username=postgres;Password=postgres");

            return new AccountDbContext(optionsBuilder.Options);
        }
    }
}
