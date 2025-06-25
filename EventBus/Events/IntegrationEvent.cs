using Mediator;

namespace EventBus.Events;
public class IntegrationEvent: INotification
{
    public Guid EventId { get; private set; }
    public DateTime EventCreationDate { get; private set; }
    public IntegrationEvent()
    {
        EventId = Guid.CreateVersion7();
        EventCreationDate = DateTime.UtcNow;
    }
}
