using EventBus.Events;

namespace Saga.OnlineStore.IntegrationEvents;
public class OrderPlacedIntegrationEvent: IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public string ShippingAddress { get; set; } = default!;
    public string PaymentCardNumber { get; set; } = default!;
    public string PaymentCardName { get; set; } = default!;
    public string PaymentCardExpiration { get; set; } = default!;
    public string PaymentCardCvv { get; set; } = default!;
    public List<OrderItem> Items { get; set; } = [];
    public OrderPlacedIntegrationEvent()
    {
    }

    public OrderPlacedIntegrationEvent(Guid orderId, Guid customerId, string customerName, string customerPhone, string shippingAddress, string paymentCardNumber, string paymentCardName, string paymentCardExpiration, string paymentCardCvv, List<OrderItem> items)
    {
        OrderId = orderId;
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerPhone = customerPhone;
        ShippingAddress = shippingAddress;
        PaymentCardNumber = paymentCardNumber;
        PaymentCardName = paymentCardName;
        PaymentCardExpiration = paymentCardExpiration;
        PaymentCardCvv = paymentCardCvv;
        Items = items;
    }

    public OrderPlacedIntegrationEvent(OrderPlacedIntegrationEvent other)
    {
        OrderId = other.OrderId;
        CustomerId = other.CustomerId;
        CustomerName = other.CustomerName;
        CustomerPhone = other.CustomerPhone;
        ShippingAddress = other.ShippingAddress;
        PaymentCardNumber = other.PaymentCardNumber;
        PaymentCardName = other.PaymentCardName;
        PaymentCardExpiration = other.PaymentCardExpiration;
        PaymentCardCvv = other.PaymentCardCvv;
        Items = other.Items;
    }
}

public class OrderItemsReservedIntegrationEvent(OrderPlacedIntegrationEvent other) : OrderPlacedIntegrationEvent(other)
{
}

public class OrderApprovedIntegrationEvent : OrderPlacedIntegrationEvent
{
}

public class OrderItemsReservationFailedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = default!;
}

public class OrderItemsReleasedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
}

public class OrderPaymentApprovedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
}

public class OrderPaymentRejectedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = default!;
}

public class OrderRejectedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = default!;
}

public enum OrderStatus
{
    Pending,
    Rejected,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
