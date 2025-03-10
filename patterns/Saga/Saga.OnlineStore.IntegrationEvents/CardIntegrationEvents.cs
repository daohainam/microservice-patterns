using EventBus.Events;

namespace Saga.OnlineStore.IntegrationEvents;
public class CardCreatedIntegrationEvent : IntegrationEvent
{
    public Guid CardId { get; set; }
    public string CardNumber { get; set; } = default!;
    public string CardHolderName { get; set; } = default!;
    public string ExpirationDate { get; set; } = default!;
    public string Cvv { get; set; } = default!;
}

public class CardDeletedIntegrationEvent : IntegrationEvent
{
    public Guid CardId { get; set; }
}

public class CardBalanceChangedIntegrationEvent : IntegrationEvent
{
    public Guid CardId { get; set; }
    public decimal Balance { get; set; }
}
