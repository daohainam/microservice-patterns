using EventBus.Events;

namespace Saga.OnlineStore.IntegrationEvents;
public class ProductCreatedIntegrationEvent: IntegrationEvent
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
}

public class ProductUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
}
