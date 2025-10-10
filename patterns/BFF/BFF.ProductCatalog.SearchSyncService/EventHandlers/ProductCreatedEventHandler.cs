using Elastic.Clients.Elasticsearch;
using System.Text.Json;

namespace BFF.ProductCatalog.SearchSyncService.EventHandlers;
internal class ProductCreatedEventHandler(ElasticsearchClient client, ILogger<ProductCreatedEvent> logger) : INotificationHandler<ProductCreatedEvent>
{
    public async Task Handle(ProductCreatedEvent evt, CancellationToken cancellationToken)
    {
        {
            try
            {
                var doc = ProductEsMapper.Map(evt);

                // serialize doc to json string is not necessary, just for logging purpose, should be removed in production
                logger.LogInformation("Indexing product {id} to Elasticsearch, document: {doc}", evt.ProductId, JsonSerializer.Serialize(doc));
                var response = await client.IndexAsync(doc, cancellationToken: cancellationToken);

                if (!response.IsValidResponse)
                {
                    logger.LogInformation("Error indexing product {id}, {err}", evt.ProductId, response.ElasticsearchServerError);
                }
                else
                {
                    logger.LogInformation("Successfully indexed product {id}", evt.ProductId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error indexing product {id}", evt.ProductId);
                throw;
            }
        }
    }
}
