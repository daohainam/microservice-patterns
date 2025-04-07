namespace WebHook.DeliveryService.Infrastructure.Entity;
public class WebHook
{
    public Guid Id { get; set; }
    public string Url { get; set; } = default!;
    public string WebHookEventTypeId { get; set; } = default!;
    public WebHookEventType WebHookEventType { get; set; } = default!;
}
