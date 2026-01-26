using EventBus.Events;

namespace EventBus.Kafka;
public class EventHandlingService(IConsumer<string, MessageEnvelop> consumer,
    EventHandlingWorkerOptions options,
    IIntegrationEventFactory integrationEventFactory,
    IServiceScopeFactory serviceScopeFactory,
    ILoggerFactory loggerFactory) : BackgroundService
{
    private readonly IConsumer<string, MessageEnvelop> consumer = consumer;
    private readonly EventHandlingWorkerOptions options = options;
    private readonly IIntegrationEventFactory integrationEventFactory = integrationEventFactory;
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger logger = loggerFactory.CreateLogger(options.ServiceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subcribing to topics [{topics}]...", string.Join(',', options.Topics));

        consumer.Subscribe(options.Topics);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(100);

                        if (consumeResult is not null)
                        {
                            using IServiceScope scope = serviceScopeFactory.CreateScope();
                            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                            await ProcessMessageAsync(mediator, consumeResult.Message.Value, stoppingToken);
                        }
                        else
                        {
                            logger.LogDebug("No message consumed, waiting...");
                            await Task.Delay(100, stoppingToken);
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
                logger.LogError(ex, "Error in event handling loop");
            }

            await Task.Delay(1000, stoppingToken);
        }

    }

    private async Task ProcessMessageAsync(IMediator mediator, MessageEnvelop message, CancellationToken cancellationToken)
    {
        var @event = integrationEventFactory.CreateEvent(message.MessageTypeName, message.Message);

        if (@event is not null)
        {
            if (options.AcceptEvent(@event))
            {
                logger.LogInformation("Processing message {t}: {message}", message.MessageTypeName, message.Message);

                // here we must use a scope to resolve the mediator since a background service is registered as a singleton service
                await mediator.Publish(@event, cancellationToken);
            }
            else
            {
                logger.LogDebug("Event skipped: {t}", message.MessageTypeName);
            }
        }
        else
        {
            logger.LogWarning("Event type not found: {t}", message.MessageTypeName);
        }
    }
}

public class EventHandlingWorkerOptions
{
    public string KafkaGroupId { get; set; } = "event-handling";
    public List<string> Topics { get; set; } = [];
    public IIntegrationEventFactory IntegrationEventFactory { get; set; } = EventBus.IntegrationEventFactory.Instance;
    public string ServiceName { get; set; } = "EventHandlingService";
    public Func<IntegrationEvent, bool> AcceptEvent { get; set; } = _ => true;
}
