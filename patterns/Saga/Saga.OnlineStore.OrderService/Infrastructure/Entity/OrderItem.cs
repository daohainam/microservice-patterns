using System.Text.Json.Serialization;

namespace Saga.OnlineStore.OrderService.Infrastructure.Entity;
[PrimaryKey(nameof(OrderId), nameof(ProductId))]
public class OrderItem
{
    [JsonIgnore]
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    [JsonIgnore]
    public decimal Total => Quantity * UnitPrice;

    [JsonIgnore]
    public Order Order { get; set; } = default!;
}
