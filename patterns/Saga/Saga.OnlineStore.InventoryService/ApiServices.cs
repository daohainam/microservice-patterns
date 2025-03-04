using EventBus.Abstractions;
using Saga.OnlineStore.InventoryService.Infrastructure.Data;

namespace Saga.OnlineStore.InventoryService;
public class ApiServices(
    InventoryDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public InventoryDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
