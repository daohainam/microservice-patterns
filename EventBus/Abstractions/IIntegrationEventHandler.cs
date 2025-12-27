using EventBus.Events;

namespace EventBus.Abstractions;

/// <summary>
/// Defines a handler for a specific integration event.
/// </summary>
/// <typeparam name="T">The type of integration event to handle.</typeparam>
public interface IIntegrationEventHandler<T> where T : IntegrationEvent
{
    /// <summary>
    /// Handles the specified integration event.
    /// </summary>
    /// <param name="event">The integration event to handle.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Handle(T @event);
}