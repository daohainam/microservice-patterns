namespace WebHook.DeliveryService.Infrastructure.Entity;
public class DeliveryEventQueueItem
{
    public Guid Id { get; set; }
    public string Message { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryTimes { get; set; }
    public bool IsProcessed { get; set; }
    public bool IsSuccess { get; set; }

    public Guid WebHookSubscriptionId { get; set; } = default!;
    public WebHookSubscription WebHookSubscription { get; set; } = default!;
}
