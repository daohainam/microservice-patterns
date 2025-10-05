using Microsoft.EntityFrameworkCore.Design;

namespace BFF.ProductCatalogService.Infrastructure.Data
{
    public class ProductCatalogDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProductCatalogDbContext>
    {
        public ProductCatalogDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProductCatalogDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=productcatalog;Username=postgres;Password=postgres");

            return new ProductCatalogDbContext(optionsBuilder.Options);
        }
    }
}
