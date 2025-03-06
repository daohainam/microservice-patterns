using MicroservicePatterns.Shared;
using Saga.OnlineStore.OrderService.Infrastructure.Data;

namespace Saga.OnlineStore.OrderService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<OrderDbContext>(Consts.DefaultDatabase);
        builder.AddKafkaEventPublisher("kafka");
        builder.Services.AddKafkaEventPublisher(builder.Configuration.GetValue<string>(Consts.EnvKafkaTopic));

        return builder;
    }
}
