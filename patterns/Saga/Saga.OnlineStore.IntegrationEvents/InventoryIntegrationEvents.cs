using EventBus.Events;

namespace Saga.OnlineStore.IntegrationEvents;

public class ItemRestockedIntegrationEvent : IntegrationEvent
{
    public Guid ItemId { get; set; }
    public long QuantityBefore { get; set; }
    public long QuantityAfter { get; set; }
}



