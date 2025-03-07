namespace Saga.OnlineStore.OrderService.Infrastructure.Entity;
public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public string ShippingAddress { get; set; } = default!;
    public string PaymentCardNumber { get; set; } = default!;
    public string PaymentCardName { get; set; } = default!;
    public string PaymentCardExpiration { get; set; } = default!;
    public string PaymentCardCvv { get; set; } = default!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string StatusMessage { get; set; } = default!;
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
