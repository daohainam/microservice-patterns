namespace WebHook.DeliveryService.DomainEvents;
public abstract class DeliveryEvent
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }

    public string Sender { get; set; } = default!;
    public string Receiver { get; set; } = default!;
    public string SenderAddress { get; set; } = default!;
    public string ReceiverAddress { get; set; } = default!;
    public string PackageInfo { get; set; } = default!;
}

public class DeliveryCreatedEvent: DeliveryEvent
{
}

public class DeliveryUpdatedEvent: DeliveryEvent
{
}
