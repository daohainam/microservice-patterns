using EventBus.Abstractions;

namespace CQRS.Library.BorrowingHistoryApi;
public class ApiServices(
    BorrowingDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public BorrowingDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
