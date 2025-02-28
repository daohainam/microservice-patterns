using CQRS.Library.BorrowerApi.Infrastructure.Entity;
using EventBus.Abstractions;

namespace CQRS.Library.BorrowerApi;
public class ApiServices(
    BorrowerDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public BorrowerDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
