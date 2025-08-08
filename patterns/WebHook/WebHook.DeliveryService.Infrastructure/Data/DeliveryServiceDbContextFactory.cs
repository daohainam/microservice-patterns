using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebHook.DeliveryService.Infrastructure.Data;
public class DeliveryServiceDbContextFactory : IDesignTimeDbContextFactory<DeliveryServiceDbContext>
{
    public DeliveryServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DeliveryServiceDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=corebanking;Username=postgres;Password=postgres");

        return new DeliveryServiceDbContext(optionsBuilder.Options);
    }
}
