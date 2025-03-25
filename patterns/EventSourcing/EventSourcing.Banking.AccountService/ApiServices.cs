using EventSourcing.Infrastructure;

namespace EventSourcing.Banking.AccountService;
public class ApiServices(
    IEventStore eventStore)
{
    public IEventStore EventStore => eventStore;

}
