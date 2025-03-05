using Saga.OnlineStore.CatalogService.Infrastructure.Data;

namespace Saga.OnlineStore.CatalogService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<CatalogDbContext>("saga-onlinestore-catalog-db");
        builder.AddKafkaEventPublisher("kafka");
        builder.Services.AddKafkaEventPublisher("saga-onlinestore-catalog");

        return builder;
    }
}
