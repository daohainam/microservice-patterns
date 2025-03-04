namespace Saga.OnlineStore.InventoryService.Infrastructure.Data;
public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> Items { get; set; } = default!;
    public DbSet<ReservedItem> ReservedItems { get; set; } = default!;
}
