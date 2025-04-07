namespace WebHook.DeliveryService.DomainEvents;
public abstract class DeliveryEvent
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DeliveryCreatedEvent: DeliveryEvent
{
}

public class DeliveryLocationChangedEvent: DeliveryEvent
{
}

public class DeliveryStatusChangedEvent: DeliveryEvent
{
}
