using EventBus.Abstractions;
namespace Saga.OnlineStore.InventoryService;
public class ApiServices(
    InventoryDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<InventoryApi> logger)
{
    public InventoryDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;
    public ILogger<InventoryApi> Logger { get; } = logger;

}
