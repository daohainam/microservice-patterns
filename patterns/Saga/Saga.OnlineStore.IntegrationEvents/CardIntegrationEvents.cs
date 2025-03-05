using EventBus.Events;

namespace Saga.OnlineStore.IntegrationEvents;
public class CardCreatedIntegrationEvent : IntegrationEvent
{
    public Guid CardId { get; set; }
    public string CardNumber { get; set; } = default!;
    public string CardHolderName { get; set; } = default!;
    public DateTime ExpirationDate { get; set; }
    public string Cvv { get; set; } = default!;
}

public class CardUpdatedIntegrationEvent: IntegrationEvent
{
    public Guid CardId { get; set; }
    public string CardNumber { get; set; } = default!;
    public string CardHolderName { get; set; } = default!;
    public DateTime ExpirationDate { get; set; }
    public string Cvv { get; set; } = default!;
}

public class CardBalanceChangedIntegrationEvent : IntegrationEvent
{
    public Guid CardId { get; set; }
    public decimal Balance { get; set; }
}
