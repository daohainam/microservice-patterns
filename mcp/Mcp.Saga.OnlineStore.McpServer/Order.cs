namespace Mcp.Saga.OnlineStore.McpServer;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public string ShippingAddress { get; set; } = default!;
    public string PaymentCardNumber { get; set; } = default!;
    public string PaymentCardName { get; set; } = default!;
    public string PaymentCardExpiration { get; set; } = default!;
    public string PaymentCardCvv { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string StatusMessage { get; set; } = default!;
    public List<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
