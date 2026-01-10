using ModelContextProtocol.Server;
using System.Text.Json;

namespace Mcp.Saga.OnlineStore.McpServer;

[McpServerToolType]
public class OrderTool
{
    [McpServerTool]
    public static async Task<string> GetOrders(IOrderService orderService, CancellationToken cancellationToken)
    {
        var orders = await orderService.GetOrders(cancellationToken);
        return JsonSerializer.Serialize(orders);
    }

    [McpServerTool]
    public static async Task<string> GetOrderDetails(IOrderService orderService, string orderId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(orderId, out var parsedOrderId))
        {
            throw new ArgumentException($"Invalid order ID format: '{orderId}'. Expected a valid GUID.", nameof(orderId));
        }

        var order = await orderService.GetOrderById(parsedOrderId, cancellationToken);
        return JsonSerializer.Serialize(order);
    }

    [McpServerTool]
    public static async Task<string> CreateOrder(
        IOrderService orderService,
        string customerId,
        string customerName,
        string customerPhone,
        string shippingAddress,
        string paymentCardNumber,
        string paymentCardName,
        string paymentCardExpiration,
        string paymentCardCvv,
        string orderItemsJson,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(customerId, out var parsedCustomerId))
        {
            throw new ArgumentException($"Invalid customer ID format: '{customerId}'. Expected a valid GUID.", nameof(customerId));
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            throw new ArgumentException("Customer name is required.", nameof(customerName));
        }

        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            throw new ArgumentException("Shipping address is required.", nameof(shippingAddress));
        }

        if (string.IsNullOrWhiteSpace(paymentCardNumber))
        {
            throw new ArgumentException("Payment card number is required.", nameof(paymentCardNumber));
        }

        // Parse order items from JSON
        List<OrderItem> orderItems;
        try
        {
            orderItems = JsonSerializer.Deserialize<List<OrderItem>>(orderItemsJson) ?? [];
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid order items JSON format: {ex.Message}", nameof(orderItemsJson));
        }

        if (orderItems.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one item.", nameof(orderItemsJson));
        }

        var order = new Order
        {
            CustomerId = parsedCustomerId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            ShippingAddress = shippingAddress,
            PaymentCardNumber = paymentCardNumber,
            PaymentCardName = paymentCardName,
            PaymentCardExpiration = paymentCardExpiration,
            PaymentCardCvv = paymentCardCvv,
            Items = orderItems
        };

        var createdOrder = await orderService.CreateOrder(order, cancellationToken);
        return JsonSerializer.Serialize(createdOrder);
    }
}
