namespace WebHook.DeliveryService.Infrastructure.Entity;
public class WebHookEventType
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public List<WebHook> WebHooks { get; set; } = default!;
}
