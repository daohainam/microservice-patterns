using MicroservicePatterns.Shared;
using IdempotentConsumer.CatalogService.Infrastructure.Data;

namespace IdempotentConsumer.CatalogService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<CatalogDbContext>(Consts.DefaultDatabase);

        return builder;
    }
}
