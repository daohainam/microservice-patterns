using EventBus.Events;

namespace TransactionalOutbox.IntegrationEvents;

public class BalanceChangedIntegrationEvent: IntegrationEvent
{
    public string AccountNumber { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal Credit { get; set; }
}
