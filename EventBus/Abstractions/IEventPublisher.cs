using EventBus.Events;

namespace EventBus.Abstractions;

/// <summary>
/// Defines a publisher for integration events.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Asynchronously publishes an integration event.
    /// </summary>
    /// <typeparam name="TEvent">The type of integration event to publish.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <returns>A task that represents the asynchronous operation, containing true if the event was published successfully; otherwise, false.</returns>
    Task<bool> PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent;
}
