namespace IdempotentConsumer.CatalogService;
public class ApiServices(
    CatalogDbContext dbContext, ILogger<CatalogApi> logger)
{
    public CatalogDbContext DbContext => dbContext;
    public ILogger<CatalogApi> Logger => logger;
}
