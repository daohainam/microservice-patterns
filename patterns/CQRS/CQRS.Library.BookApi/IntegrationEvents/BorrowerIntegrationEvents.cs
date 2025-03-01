namespace CQRS.Library.BookApi.IntegrationEvents;
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
