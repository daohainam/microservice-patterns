using BFF.ProductCatalogService.Infrastructure.Data;

namespace BFF.ProductCatalogService;
public class ApiServices(
    ProductCatalogDbContext dbContext)
{
    public ProductCatalogDbContext DbContext => dbContext;
}
