using EventSourcing.Infrastructure.Models;

namespace EventSourcing.Infrastructure;
public interface IEventStore
{
    Task<Guid> AppendAsync(Guid streamId, StreamStates state, IEnumerable<Event> events);
    Task<IEnumerable<Event>> ReadAsync(Guid streamId, long? afterVersion = null);
}

public enum StreamStates
{
    New,
    Existing
}
