using EventBus.Events;

namespace Saga.OnlineStore.IntegrationEvents;
public class OrderCreatedIntegrationEvent: IntegrationEvent
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public string ShippingAddress { get; set; } = default!;
    public string PaymentCardNumber { get; set; } = default!;
    public string PaymentCardName { get; set; } = default!;
    public string PaymentCardExpiration { get; set; } = default!;
    public string PaymentCardCvv { get; set; } = default!;
    public List<OrderItem> Items { get; set; } = [];
}

public enum OrderStatus
{
    Pending,
    Created,
    Rejected,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
