namespace Saga.OnlineStore.InventoryService.EventHandlers;
public class OrderIntegrationEventHandlers(InventoryDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<ProductIntegrationEventHandlers> logger) :
    INotificationHandler<OrderPlacedIntegrationEvent>,
    INotificationHandler<OrderPaymentRejectedIntegrationEvent>
{
    public async ValueTask Handle(OrderPlacedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling order created event: {id}", request.OrderId);

        try
        {
            foreach (var requestItem in request.Items)
            {
                var itemInInventory = await dbContext.Items.Where(item => item.Id == requestItem.ProductId).SingleOrDefaultAsync(cancellationToken: cancellationToken);

                if (itemInInventory != null)
                {
                    logger.LogInformation("Reserving item {id} for order {orderId}, current quantity {q}", requestItem.ProductId, request.OrderId, itemInInventory.AvailableQuantity);

                    if (itemInInventory.AvailableQuantity >= requestItem.Quantity)
                    {
                        itemInInventory.AvailableQuantity -= requestItem.Quantity;

                        dbContext.ReservedItems.Add(new ReservedItem()
                        {
                            Id = Guid.NewGuid(),
                            ItemId = requestItem.ProductId,
                            OrderId = request.OrderId,
                            Quantity = requestItem.Quantity,
                        });
                    }
                    else
                    {
                        await eventPublisher.PublishAsync(new OrderItemsReservationFailedIntegrationEvent()
                        {
                            OrderId = request.OrderId,
                            Reason = $"Item stock too low: {requestItem.ProductId}"
                        });

                        return;
                    }
                }
                else
                {
                    await eventPublisher.PublishAsync(new OrderItemsReservationFailedIntegrationEvent()
                    {
                        OrderId = request.OrderId,
                        Reason = $"Item not in stock: {requestItem.ProductId}"
                    });

                    return;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            await eventPublisher.PublishAsync(new OrderItemsReservedIntegrationEvent(request));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reserving items for order: {id}", request.OrderId);
            await eventPublisher.PublishAsync(new OrderItemsReservationFailedIntegrationEvent()
            {
                OrderId = request.OrderId,
                Reason = $"Error reserving items: {ex.Message}"
            });
        }
    }
    public async ValueTask Handle(OrderPaymentRejectedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling order rejected event: {id}", request.OrderId);

        var reservedItems = await dbContext.ReservedItems.Where(item => item.OrderId == request.OrderId).ToListAsync(cancellationToken);
        foreach (var item in reservedItems)
        {
            var itemInInventory = await dbContext.Items.Where(itm => itm.Id == item.ItemId).SingleOrDefaultAsync(cancellationToken);
            if (itemInInventory != null)
            {
                itemInInventory.AvailableQuantity += item.Quantity;
                dbContext.ReservedItems.Remove(item);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
