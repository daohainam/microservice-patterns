namespace Saga.OnlineStore.CatalogService.Infrastructure.Data;
public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = default!;
}
