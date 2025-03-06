using EventBus.Abstractions;
using Saga.OnlineStore.OrderService.Infrastructure.Data;

namespace Saga.OnlineStore.OrderService;
public class ApiServices(
    OrderDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public OrderDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
