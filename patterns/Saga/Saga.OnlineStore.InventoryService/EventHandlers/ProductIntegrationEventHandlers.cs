namespace Saga.OnlineStore.InventoryService.EventHandlers;
public class ProductIntegrationEventHandlers(InventoryDbContext dbContext, ILogger<ProductIntegrationEventHandlers> logger) :
    IRequestHandler<ProductCreatedIntegrationEvent>,
    IRequestHandler<ProductUpdatedIntegrationEvent>
{
    public async Task Handle(ProductCreatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling product created event: {productId}", request.ProductId);
        dbContext.Items.Add(new InventoryItem
        {
            Id = request.ProductId,
            Name = request.Name,
            AvailableQuantity = 0,
            ReservedQuantity = 0            
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(ProductUpdatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling product updated event: {productId}", request.ProductId);
        await dbContext.Items.Where(x => x.Id == request.ProductId).ExecuteUpdateAsync(setters => setters.SetProperty(b => b.Name, request.Name), cancellationToken: cancellationToken);
    }
}
