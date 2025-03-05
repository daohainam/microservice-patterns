namespace Saga.OnlineStore.CatalogService.Infrastructure.Data;
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = default!;
}
