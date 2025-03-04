namespace Saga.OnlineStore.InventoryService.Infrastructure.Entity;

public class ReservedItem
{
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
    public long Quantity { get; set; }
}
