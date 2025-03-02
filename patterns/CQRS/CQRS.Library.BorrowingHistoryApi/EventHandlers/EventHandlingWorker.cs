using Confluent.Kafka;
using EventBus.Abstractions;

namespace CQRS.Library.BorrowingHistoryApi.EventHandlers;
public class EventHandlingWorker(IConsumer<string, MessageEnvelop> consumer, EventHandlingWorkerOptions options, ILogger<EventHandlingWorker> logger) : BackgroundService
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
                            logger.LogInformation("Consumed message: {message}", consumeResult.Message.Value);
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
}

public class EventHandlingWorkerOptions
{
    public string BookTopic { get; set; } = "book";
    public string BorrowerTopic { get; set; } = "borrower";
    public string BorrowingTopic { get; set; } = "borrowing";
}
