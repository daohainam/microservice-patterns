using EventSourcing.Infrastructure;

namespace EventSourcing.Banking.AccountService;
public class ApiServices(
    IEventStore eventStore, CancellationToken cancellationToken)
{
    public IEventStore EventStore => eventStore;
    public CancellationToken CancellationToken => cancellationToken;

}
