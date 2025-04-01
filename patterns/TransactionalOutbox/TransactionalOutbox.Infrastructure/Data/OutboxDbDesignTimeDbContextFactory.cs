using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TransactionalOutbox.Infrastructure.Data;

namespace TransactionalOutbox.Banking.AccountService.Infrastructure.Data
{
    public class OutboxDbDesignTimeDbContextFactory: IDesignTimeDbContextFactory<OutboxDbContext>
    {
        public OutboxDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OutboxDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=TransactionalOutbox;Username=postgres;Password=postgres");

            return new OutboxDbContext(optionsBuilder.Options);
        }
    }
}
