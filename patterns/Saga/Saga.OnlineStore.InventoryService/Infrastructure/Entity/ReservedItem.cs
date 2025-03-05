using System;

namespace Saga.OnlineStore.InventoryService.Infrastructure.Entity;

[Index(nameof(OrderId), nameof(ItemId), IsUnique = true)]
public class ReservedItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
    public long Quantity { get; set; }
}
