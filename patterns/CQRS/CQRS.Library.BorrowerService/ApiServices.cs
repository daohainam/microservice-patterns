using CQRS.Library.BorrowerService.Infrastructure.Data;
using EventBus.Abstractions;

namespace CQRS.Library.BorrowerService;
public class ApiServices(
    BorrowerDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public BorrowerDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
