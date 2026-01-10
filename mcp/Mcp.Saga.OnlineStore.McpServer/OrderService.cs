namespace Mcp.Saga.OnlineStore.McpServer;

public class OrderService(HttpClient orderHttpClient) : IOrderService
{
    public async Task<IEnumerable<Order>> GetOrders(CancellationToken cancellationToken = default)
    {
        var orders = await orderHttpClient.GetFromJsonAsync<IEnumerable<Order>>("/api/saga/v1/orders", cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve orders from the order service.");
        return orders;
    }

    public async Task<Order> GetOrderById(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await orderHttpClient.GetFromJsonAsync<Order>($"/api/saga/v1/orders/{orderId}", cancellationToken: cancellationToken) ?? throw new InvalidOperationException($"Failed to retrieve order {orderId}");
        return order;
    }

    public async Task<Order> CreateOrder(Order order, CancellationToken cancellationToken = default)
    {
        var response = await orderHttpClient.PostAsJsonAsync("/api/saga/v1/orders", order, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        var createdOrder = await response.Content.ReadFromJsonAsync<Order>(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to create order.");
        return createdOrder;
    }
}
