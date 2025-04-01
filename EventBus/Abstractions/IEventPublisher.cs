using EventBus.Events;

namespace EventBus.Abstractions;
public interface IEventPublisher
{
    Task<bool> PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent;
}
