using Microsoft.Extensions.Hosting;

namespace BFF.ProductCatalogService.Infrastructure.Data;
public class ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; internal set; } = default!;
    public DbSet<ProductDimension> ProductDimensions { get; internal set; } = default!;
    public DbSet<Dimension> Dimensions { get; internal set; }
    public DbSet<DimensionValue> DimensionValues { get; internal set; }
    public DbSet<Category> Categories { get; internal set; }
    public DbSet<Group> Groups { get; internal set; }
    public DbSet<Variant> Variants { get; internal set; }
    public DbSet<VariantDimensionValue> VariantDimensionValues { get; internal set; }
    public DbSet<ProductImage> ProductImages { get; internal set; }
    public DbSet<Image> Images { get; internal set; }
    public DbSet<Brand> Brands { get; internal set; }
    public DbSet<GroupProduct> GroupProducts { get; internal set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasMany(e => e.Groups)
            .WithMany(e => e.Products)
            .UsingEntity<GroupProduct>();
    }
}
