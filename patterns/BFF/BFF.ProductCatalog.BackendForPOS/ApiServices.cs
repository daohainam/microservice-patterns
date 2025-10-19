using Elastic.Clients.Elasticsearch;

namespace BFF.ProductCatalog.BackendForPOS;
public class ApiServices(ElasticsearchClient client, CancellationToken cancellationToken)
{
    public ElasticsearchClient Client => client;
    public CancellationToken CancellationToken => cancellationToken;
}
