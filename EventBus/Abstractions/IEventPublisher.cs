using EventBus.Events;

namespace EventBus.Abstractions;
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent: IntegrationEvent;
}
