namespace Saga.OnlineStore.InventoryService.EventHandlers;
public class OrderIntegrationEventHandlers(OrderDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<OrderIntegrationEventHandlers> logger) :
    INotificationHandler<OrderItemsReservationFailedIntegrationEvent>,
    INotificationHandler<OrderPaymentApprovedIntegrationEvent>,
    INotificationHandler<OrderPaymentRejectedIntegrationEvent>
{
    public async ValueTask Handle(OrderItemsReservationFailedIntegrationEvent request, CancellationToken cancellationToken)
    {
        // this event is sent by Inventory service when it fails to reserve items for an order
        logger.LogInformation("Handling order reservation failed event: {id}", request.OrderId);

        await RejectOrder(request.OrderId, request.Reason, cancellationToken);
    }

    public async ValueTask Handle(OrderPaymentRejectedIntegrationEvent request, CancellationToken cancellationToken)
    {
        // this event is sent by Payment service when it rejects payment for an order
        logger.LogInformation("Handling order payment rejected event: {id}", request.OrderId);
    
        await RejectOrder(request.OrderId, "Payment rejected", cancellationToken);
    }

    private async ValueTask RejectOrder(Guid orderId, string reason, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.Where(o => o.Id == orderId).SingleOrDefaultAsync(cancellationToken);
        if (order == null)
        {
            logger.LogWarning("Order not found: {id}", orderId);
            return;
        }
        order.Status = OrderService.Infrastructure.Entity.OrderStatus.Rejected;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order {id} rejected: {reason}", orderId, reason);

        await eventPublisher.PublishAsync(new OrderRejectedIntegrationEvent()
        {
            OrderId = orderId,
            Reason = reason
        });
    }

    public async ValueTask Handle(OrderPaymentApprovedIntegrationEvent request, CancellationToken cancellationToken)
    {
        // this event is sent by Payment service when it approves payment for an order
        logger.LogInformation("Handling order payment approved event: {id}", request.OrderId);
        var order = await dbContext.Orders.Where(o => o.Id == request.OrderId).SingleOrDefaultAsync(cancellationToken);
        if (order == null)
        {
            logger.LogWarning("Order not found: {id}", request.OrderId);
            return;
        }

        order.Status = OrderService.Infrastructure.Entity.OrderStatus.Created;
        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new OrderApprovedIntegrationEvent()
        {
            OrderId = request.OrderId,            
        });
    }
}
