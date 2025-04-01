using EventBus.Events;

namespace EventBus.Kafka;
public class KafkaEventPublisher(string topic, IProducer<string, MessageEnvelop> producer, ILogger logger) : IEventPublisher
{
    public async Task<bool> PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent
    {
        var json = JsonSerializer.Serialize(@event, @event.GetType());
        logger.LogInformation("Publishing event {type} to topic {topic}: {event}", @event.GetType().Name, topic, json);

        try
        {
            await producer.ProduceAsync(topic, new Message<string, MessageEnvelop> { Key = @event.GetType().FullName!, 
                Value = new MessageEnvelop(typeof(TEvent), json) }
            );

            logger.LogInformation("Published event {@event}", @event.EventId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing event {@event}", @event.EventId);

            return false;
        }
    }
}
