using EventSourcing.Infrastructure.Models;

namespace EventSourcing.Infrastructure;

/// <summary>
/// Defines an event store for persisting and retrieving event streams.
/// </summary>
/// <remarks>
/// The event store is the central persistence mechanism in Event Sourcing.
/// It stores events in ordered streams, typically one stream per aggregate instance.
/// </remarks>
public interface IEventStore
{
    /// <summary>
    /// Appends new events to a stream.
    /// </summary>
    /// <param name="streamId">The unique identifier of the stream (typically the aggregate ID).</param>
    /// <param name="state">Indicates whether this is a new stream or an existing one.</param>
    /// <param name="events">The events to append to the stream.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the new version number of the stream.</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to append to a new stream that already exists, or vice versa.</exception>
    Task<long> AppendAsync(Guid streamId, StreamStates state, IEnumerable<Event> events, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reads events from a stream.
    /// </summary>
    /// <param name="streamId">The unique identifier of the stream to read from.</param>
    /// <param name="afterVersion">Optional version number. If specified, only events after this version are returned.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the list of events in the stream.</returns>
    Task<List<Event>> ReadAsync(Guid streamId, long? afterVersion = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the state of an event stream when appending events.
/// </summary>
public enum StreamStates
{
    /// <summary>
    /// Indicates a new stream that doesn't exist yet.
    /// </summary>
    New,
    
    /// <summary>
    /// Indicates an existing stream that already has events.
    /// </summary>
    Existing
}
