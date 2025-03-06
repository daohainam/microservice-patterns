namespace Saga.OnlineStore.InventoryService.EventHandlers;
public class OrderIntegrationEventHandlers(InventoryDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<ProductIntegrationEventHandlers> logger) :
    IRequestHandler<OrderCreatedIntegrationEvent>
{
    public async Task Handle(OrderCreatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling order created event: {id}", request.OrderId);

        foreach (var item in request.Items) { 
            var itemInInventory = await dbContext.Items.Where(itm => itm.Id == item.ProductId).SingleOrDefaultAsync(cancellationToken);

            if (itemInInventory != null)
            {
                if (itemInInventory.AvailableQuantity >= item.Quantity) {
                    itemInInventory.AvailableQuantity -= item.Quantity;
                    itemInInventory.ReservedQuantity += item.Quantity;

                    dbContext.ReservedItems.Add(new ReservedItem() { 
                        Id = Guid.NewGuid(),
                        ItemId = item.ProductId,
                        OrderId = item.ProductId,
                        Quantity = item.Quantity,
                    });
                }
                else
                {
                    await eventPublisher.PublishAsync(new ItemsReservationFailedIntegrationEvent()
                    {
                        OrderId = request.OrderId,
                        ItemId = item.ProductId,
                        Reason = "Not enough item in inventory"
                    });

                    return;
                }
            }
            else
            {
                await eventPublisher.PublishAsync(new ItemsReservationFailedIntegrationEvent() { 
                    OrderId = request.OrderId,
                    ItemId = item.ProductId,
                    Reason = "Item not found"
                });

                return;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new ItemsReservedIntegrationEvent()
        {
            OrderId = request.OrderId,
        });

    }
}
