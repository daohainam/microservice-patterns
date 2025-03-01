namespace EventBus.Events;
public class IntegrationEvent
{
    public Guid EventId { get; private set; }
    public DateTime CreationDate { get; private set; }
    public IntegrationEvent()
    {
        EventId = Guid.CreateVersion7();
        CreationDate = DateTime.UtcNow;
    }
}
