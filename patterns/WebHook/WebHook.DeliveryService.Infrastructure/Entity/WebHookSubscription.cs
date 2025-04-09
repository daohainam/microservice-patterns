namespace WebHook.DeliveryService.Infrastructure.Entity;
public class WebHookSubscription
{
    public Guid Id { get; set; }
    public string Url { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
}
