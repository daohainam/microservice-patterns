using EventSourcing.Infrastructure.Models;

namespace EventSourcing.Infrastructure;
public interface IEventStream
{
    Task<Guid> AppendAsync(Guid streamId, StreamStates state, IEnumerable<Event> events);
    Task<IEnumerable<Event>> ReadAsync(Guid streamId, Guid? fromId = null);
}

public enum StreamStates
{
    NoStream,
    Existing
}
