namespace Saga.OnlineStore.InventoryService.EventHandling;
public class EventHandlingService(IConsumer<string, MessageEnvelop> consumer,
    EventHandlingWorkerOptions options,
    IIntegrationEventFactory integrationEventFactory,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<EventHandlingService> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subcribing to topics ...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                consumer.Subscribe([options.Topic]);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(200);

                        if (consumeResult != null)
                        {
                            await ProcessMessageAsync(consumeResult.Message.Value, stoppingToken);
                        }
                        else
                        {
                            await Task.Delay(200, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error consuming message");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error subscribing to topics");
            }

            await Task.Delay(1000, stoppingToken);
        }

    }

    private async Task ProcessMessageAsync(MessageEnvelop message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing message: {message}", message.Message);

        var @event = integrationEventFactory.CreateEvent(message.MessageTypeName, message.Message);

        if (@event is not null)
        {
            // here we must use a scope to resolve the mediator since a background service is registered as a singleton service
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(@event, cancellationToken);
        }
        else
        {
            logger.LogWarning("Event type not found: {t}", message.MessageTypeName);
        }
    }
}

public class EventHandlingWorkerOptions
{
    public List<string> Topics { get; set; } = [];
}
