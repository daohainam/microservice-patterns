using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFF.ProductCatalog.SearchSyncService.EventHandlers;
internal class ProductCreatedEventHandler(ILogger<ProductCreatedEvent> logger) : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Product created");
        return Task.FromResult(0);        
    }
}
