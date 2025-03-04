using CQRS.Library.BorrowingService.Infrastructure.Data;
using EventBus.Abstractions;

namespace CQRS.Library.BorrowingService;
public class ApiServices(
    BorrowingDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public BorrowingDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
