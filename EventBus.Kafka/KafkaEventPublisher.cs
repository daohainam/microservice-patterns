using EventBus.Abstractions;
using EventBus.Events;

namespace EventBus.Kafka;
public class KafkaEventPublisher : IEventPublisher
{
    public Task PublishAsync(IntegrationEvent @event)
    {
        throw new NotImplementedException();
    }
}
