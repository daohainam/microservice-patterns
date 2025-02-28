namespace EventBus.Events;
public record IntegrationEvent
{
    public Guid Id { get; private set; }
    public DateTime CreationDate { get; private set; }
    public IntegrationEvent()
    {
        Id = Guid.CreateVersion7();
        CreationDate = DateTime.UtcNow;
    }
}
