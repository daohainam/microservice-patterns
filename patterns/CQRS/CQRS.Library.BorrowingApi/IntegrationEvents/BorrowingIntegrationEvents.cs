namespace CQRS.Library.BorrowingApi.IntegrationEvents;
public class BookBorrowedIntegrationEvent: IntegrationEvent
{
    public Guid BorrowingId { get; set; }
    public Guid BorrowerId { get; set; }
    public Guid BookId { get; set; }
    public DateTime BorrowedAt { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class BookReturnedIntegrationEvent : IntegrationEvent
{
    public Guid BorrowingId { get; set; }
    public Guid BorrowerId { get; set; }
    public Guid BookId { get; set; }
    public DateTime ReturnedAt { get; set; }
}
