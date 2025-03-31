using EventSourcing.Infrastructure;

namespace EventSourcing.Banking.AccountService;
public class ApiServices(
    IEventStore eventStore,
    ILogger<AccountApi> logger,
    CancellationToken cancellationToken)
{
    public IEventStore EventStore => eventStore;
    public ILogger<AccountApi> Logger => logger;
    public CancellationToken CancellationToken => cancellationToken;

}
