namespace Mcp.Saga.OnlineStore.McpServer;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetOrders(CancellationToken cancellationToken = default);
    Task<Order> GetOrderById(Guid orderId, CancellationToken cancellationToken = default);
    Task<Order> CreateOrder(Order order, CancellationToken cancellationToken = default);
}
