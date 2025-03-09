namespace EventBus.Kafka;
public class EventHandlingService : BackgroundService
{
    private readonly IConsumer<string, MessageEnvelop> consumer;
    private readonly EventHandlingWorkerOptions options;
    private readonly IIntegrationEventFactory integrationEventFactory;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger logger;

    public EventHandlingService(IConsumer<string, MessageEnvelop> consumer,
        EventHandlingWorkerOptions options,
        IIntegrationEventFactory integrationEventFactory,
        IServiceScopeFactory serviceScopeFactory,
        ILoggerFactory loggerFactory)
    {
        this.consumer = consumer;
        this.options = options;
        this.integrationEventFactory = integrationEventFactory;
        this.serviceScopeFactory = serviceScopeFactory;

        logger = loggerFactory.CreateLogger(options.ServiceName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subcribing to topics [{topics}]...", string.Join(',', options.Topics));

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                consumer.Subscribe(options.Topics);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(200);

                        if (consumeResult != null)
                        {
                            await ProcessMessageAsync(mediator, consumeResult.Message.Value, stoppingToken);
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

    private async Task ProcessMessageAsync(IMediator mediator, MessageEnvelop message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing message {t}: {message}", message.MessageTypeName, message.Message);

        var @event = integrationEventFactory.CreateEvent(message.MessageTypeName, message.Message);

        if (@event is not null)
        {
            // here we must use a scope to resolve the mediator since a background service is registered as a singleton service
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
    public string KafkaGroupId { get; set; } = "event-handling";
    public List<string> Topics { get; set; } = [];
    public IIntegrationEventFactory IntegrationEventFactory { get; set; } = EventBus.IntegrationEventFactory.Instance;
    public string ServiceName { get; set; } = "EventHandlingService";
}
