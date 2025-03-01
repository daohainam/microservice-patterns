using Confluent.Kafka;

namespace CQRS.Library.BorrowingHistoryApi.EventHandlers;
public class EventHandlingWorker(IConsumer<string, string> consumer, EventHandlingWorkerOptions options, ILogger<EventHandlingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subcribing to topic ...");
        consumer.Subscribe("borrowing-history");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}

public class EventHandlingWorkerOptions
{
    public string BookTopic { get; set; } = "book";
    public string BorrowerTopic { get; set; } = "borrower";
    public string BorrowingTopic { get; set; } = "borrowing";
}
