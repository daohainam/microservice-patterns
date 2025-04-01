using EventBus.Events;

namespace TransactionalOutbox.IntegrationEvents;

public class AccountOpenedIntegrationEvent: IntegrationEvent
{
    public string AccountNumber { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal CreditLimit { get; set; }
}
