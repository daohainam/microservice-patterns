using EventBus.Events;

namespace EventBus;

/// <summary>
/// Factory for creating integration event instances from their serialized form.
/// </summary>
public interface IIntegrationEventFactory
{
    /// <summary>
    /// Creates an integration event instance from its type name and serialized value.
    /// </summary>
    /// <param name="typeName">The fully qualified type name of the event.</param>
    /// <param name="value">The serialized JSON value of the event.</param>
    /// <returns>An integration event instance, or null if the type cannot be resolved.</returns>
    IntegrationEvent? CreateEvent(string typeName, string value);
}
