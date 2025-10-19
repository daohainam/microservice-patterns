using BFF.ProductCatalog.Search;

namespace BFF.ProductCatalog.BackendForPOS.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        builder.AddElasticsearchClient(connectionName: "elasticsearch",
            configureClientSettings: (settings) =>
            {
                settings.DefaultMappingFor<ProductIndexDocument>(m => m.IndexName(nameof(ProductIndexDocument).ToLower()));
            }
        );

        return builder;
    }
}
