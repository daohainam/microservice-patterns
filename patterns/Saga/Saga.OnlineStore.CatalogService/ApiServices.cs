using EventBus.Abstractions;
using Saga.OnlineStore.CatalogService.Infrastructure.Data;

namespace Saga.OnlineStore.CatalogService;
public class ApiServices(
    CatalogDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public CatalogDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
