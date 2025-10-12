using BFF.ProductCatalog.Search;

namespace BFF.ProductCatalogService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        if (builder.Environment.IsDevelopment())
            builder.Services.AddOpenApi();

        builder.AddElasticsearchClient(connectionName: "elasticsearch",
            configureClientSettings: (settings) =>
            {
                settings.DefaultMappingFor<ProductIndexDocument>(m => m.IndexName(nameof(ProductIndexDocument).ToLower()));
            }
        );

        return builder;
    }
}
