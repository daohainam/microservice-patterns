using EventSourcing.Infrastructure.Models;

namespace EventSourcing.Infrastructure;
public interface IEventStore
{
    Task<long> AppendAsync(Guid streamId, StreamStates state, IEnumerable<Event> events, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> ReadAsync(Guid streamId, long? afterVersion = null, CancellationToken cancellationToken = default);
}

public enum StreamStates
{
    New,
    Existing
}
