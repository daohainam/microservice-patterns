namespace Saga.OnlineStore.InventoryService.Infrastructure.Entity;
public class InventoryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public long AvailableQuantity { get; set; }
    public long ReservedQuantity { get; set; }
}
