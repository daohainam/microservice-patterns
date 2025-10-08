using BFF.ProductCatalog.Events;
using BFF.ProductCatalog.Search;
using Elastic.Clients.Elasticsearch;
using Mediator;

namespace BFF.ProductCatalog.ElasticSyncService.EventHandlers;

internal class ProductCreatedEventHandler(ElasticsearchClient client, ILogger<ProductCreatedEventHandler> logger) : INotificationHandler<ProductCreatedEvent>
{
    public async Task Handle(ProductCreatedEvent evt, CancellationToken cancellationToken)
    {
        try
        {
            var doc = ProductEsMapper.Map(evt);

            var response = await client.IndexAsync(doc, cancellationToken: cancellationToken);

            if (!response.IsValidResponse)
            {
                logger.LogInformation("Error indexing product {id}, {err}", evt.ProductId, response.ElasticsearchServerError);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error indexing product {id}", evt.ProductId);
            throw;
        }
    }
}
