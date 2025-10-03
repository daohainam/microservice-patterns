using BFF.ProductCatalogService.Infrastructure.Entity;

namespace BFF.ProductCatalogService.Infrastructure.Data;
public class ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; internal set; } = default!;
    public DbSet<ProductDimention> ProductDimentions { get; internal set; } = default!;
    public DbSet<Dimension> Dimensions { get; internal set; }
    public DbSet<DimensionValue> DimensionValues { get; internal set; }
    public DbSet<Category> Categories { get; internal set; }
    public DbSet<Group> Groups { get; internal set; }
    public DbSet<Variant> Variants { get; internal set; }
    public DbSet<VariantDimensionValue> VariantDimensionValues { get; internal set; }
    public DbSet<ProductImage> ProductImages { get; internal set; }
    public DbSet<Image> Images { get; internal set; }
    public DbSet<Brand> Brands { get; internal set; }
}
