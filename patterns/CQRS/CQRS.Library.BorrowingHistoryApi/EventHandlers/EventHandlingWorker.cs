namespace CQRS.Library.BorrowingHistoryApi.EventHandlers;
public class EventHandlingWorker(ILogger<EventHandlingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subcribing to topic ...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
