using IdempotentConsumer.CatalogService.Infrastructure.Entity;

namespace IdempotentConsumer.CatalogService.Infrastructure.Data;
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; } = default!;
}
