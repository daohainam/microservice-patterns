namespace Saga.OnlineStore.InventoryService.EventHandlers;
public class ProductIntegrationEventHandlers(InventoryDbContext dbContext, ILogger<ProductIntegrationEventHandlers> logger) :
    INotificationHandler<ProductCreatedIntegrationEvent>
{
    public async Task Handle(ProductCreatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling product created event: {productId}", request.ProductId);
        dbContext.Items.Add(new InventoryItem
        {
            Id = request.ProductId,
            AvailableQuantity = 0
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
