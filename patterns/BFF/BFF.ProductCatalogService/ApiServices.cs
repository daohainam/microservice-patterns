using BFF.ProductCatalogService.Infrastructure.UoW;

namespace BFF.ProductCatalogService;
public class ApiServices(
    ProductCatalogDbContext dbContext, CancellationToken cancellationToken)
{
    public ProductCatalogDbContext DbContext => dbContext;
    public CancellationToken CancellationToken => cancellationToken;
}

public class ApiServicesWithUnitOfWork(
    IUnitOfWork unitOfWork, CancellationToken cancellationToken)
{
    public IUnitOfWork UnitOfWork => unitOfWork;
    public CancellationToken CancellationToken => cancellationToken;
}
