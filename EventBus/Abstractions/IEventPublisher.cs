using EventBus.Events;

namespace EventBus.Abstractions;
public interface IEventPublisher
{
    Task PublishAsync(IntegrationEvent @event);
}
