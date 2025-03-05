using EventBus;
using Saga.OnlineStore.IntegrationEvents;

namespace Saga.OnlineStore.InventoryService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<InventoryDbContext>("saga-onlinestore-inventory-db");
        builder.AddKafkaEventPublisher("kafka");
        builder.Services.AddKafkaEventPublisher("saga-onlinestore-inventory");
        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
        builder.AddKafkaEventConsumer(options => {
            options.Topics.Add("saga-onlinestore-catalog");

            options.IntegrationEventFactory = IntegrationEventFactory<ProductCreatedIntegrationEvent>.Instance;
        });

        return builder;
    }

}
