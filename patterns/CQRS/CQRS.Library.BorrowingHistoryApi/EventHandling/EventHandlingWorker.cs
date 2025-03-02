using Confluent.Kafka;
using EventBus.Abstractions;
using MediatR;
using System.Net.NetworkInformation;

namespace CQRS.Library.BorrowingHistoryApi.EventHandling;
public class EventHandlingWorker(IConsumer<string, MessageEnvelop> consumer,
    EventHandlingWorkerOptions options,
    IIntegrationEventFactory integrationEventFactory,
    IServiceScopeFactory serviceScopeFactory,
    //IMediator mediator,
    ILogger<EventHandlingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subcribing to topics ...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                consumer.Subscribe([options.BookTopic, options.BorrowingTopic, options.BorrowerTopic]);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);

                        if (consumeResult != null)
                        {
                            await ProcessMessageAsync(consumeResult.Message.Value, stoppingToken);
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
    public string BookTopic { get; set; } = "book";
    public string BorrowerTopic { get; set; } = "borrower";
    public string BorrowingTopic { get; set; } = "borrowing";
}
