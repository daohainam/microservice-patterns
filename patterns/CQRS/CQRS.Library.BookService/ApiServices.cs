using CQRS.Library.BookService.Infrastructure.Data;
using EventBus.Abstractions;

namespace CQRS.Library.BookService;
public class ApiServices(
    BookDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public BookDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
