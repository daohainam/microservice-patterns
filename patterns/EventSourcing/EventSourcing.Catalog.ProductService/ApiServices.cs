using EventSourcing.Catalog.ProductService.Infrastructure.Data;
using EventBus.Abstractions;

namespace EventSourcing.Catalog.ProductService;
public class ApiServices(
    BookDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public BookDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
