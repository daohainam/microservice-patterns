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
        builder.AddEventConsumer();

        return builder;
    }

    private static IHostApplicationBuilder AddEventConsumer(this IHostApplicationBuilder builder)
    {
        builder.AddKafkaMessageEnvelopConsumer("saga-onlinestore");
        builder.Services.AddSingleton(new EventHandlingWorkerOptions());
        builder.Services.AddSingleton<IIntegrationEventFactory, IntegrationEventFactory<ItemRestockedIntegrationEvent>>();
        builder.Services.AddHostedService<EventHandlingService>();
        return builder;
    }

}
