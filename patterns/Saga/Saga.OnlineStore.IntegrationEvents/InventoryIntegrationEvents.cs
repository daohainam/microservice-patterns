using EventBus.Events;

namespace Saga.OnlineStore.IntegrationEvents;
public class ItemsReservedIntegrationEvent: IntegrationEvent
{
    public Guid OrderId { get; set; }
}
public class ItemsReleasedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
}
public class ItemRestockedIntegrationEvent : IntegrationEvent
{
    public Guid ItemId { get; set; }
    public long QuantityBefore { get; set; }
    public long QuantityAfter { get; set; }
}

public class ItemsReservationFailedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
    public string Reason { get; set; } = default!;
}


