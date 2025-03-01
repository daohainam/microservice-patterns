namespace CQRS.Library.BorrowerApi.IntegrationEvents;
public class BorrowerCreatedIntegrationEvent: IntegrationEvent
{
    public Guid BorrowerId { get; set; }
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
}

public class BorrowerUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid BorrowerId { get; set; }
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
}
