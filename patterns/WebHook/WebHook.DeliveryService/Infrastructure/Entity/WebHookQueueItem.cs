namespace WebHook.DeliveryService.Infrastructure.Entity;
public class WebHookQueueItem
{
    public Guid Id { get; set; }
    public string Message { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryTimes { get; set; }
    public bool IsSuccess { get; set; }

    public string WebHookId { get; set; } = default!;
    public WebHook WebHook { get; set; } = default!;
}
