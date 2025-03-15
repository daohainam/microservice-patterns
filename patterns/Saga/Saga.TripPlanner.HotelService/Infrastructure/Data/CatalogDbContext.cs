namespace Saga.TripPlanner.HotelService.Infrastructure.Data;
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = default!;
}
