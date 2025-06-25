using EventBus.Events;
using Mediator;

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

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                consumer.Subscribe(options.Topics);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(100);

                        if (consumeResult != null)
                        {
                            using IServiceScope scope = serviceScopeFactory.CreateScope();
                            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                            await ProcessMessageAsync(mediator, consumeResult.Message.Value, stoppingToken);
                        }
                        else
                        {
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
                logger.LogError(ex, "Error subscribing to topics");
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
                await mediator.Send(@event, cancellationToken);
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
