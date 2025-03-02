using EventBus.Events;

namespace CQRS.Library.IntegrationEvents;
public class BookCreatedIntegrationEvent: IntegrationEvent
{
    public Guid BookId { get; set; }
    public string Title { get; set; } = default!;
    public string Author { get; set; } = default!;
}

public class BookUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid BookId { get; set; }
    public string Title { get; set; } = default!;
    public string Author { get; set; } = default!;
}
