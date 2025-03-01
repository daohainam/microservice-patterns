using EventBus.Abstractions;

namespace CQRS.Library.BookApi;
public class ApiServices(
    BookDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public BookDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
