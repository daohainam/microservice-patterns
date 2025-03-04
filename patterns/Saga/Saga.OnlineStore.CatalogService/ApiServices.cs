using EventBus.Abstractions;
using Saga.OnlineStore.CatalogService.Infrastructure.Data;

namespace Saga.OnlineStore.CatalogService;
public class ApiServices(
    ProductDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public ProductDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
