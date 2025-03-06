namespace Saga.OnlineStore.InventoryService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<InventoryDbContext>(Consts.DefaultDatabase);
        builder.AddKafkaEventPublisher("kafka");
        builder.Services.AddKafkaEventPublisher(builder.Configuration.GetValue<string>(Consts.EnvKafkaTopic));
        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
        builder.AddKafkaEventConsumer(options => {
            options.Topics.Add(builder.Configuration.GetValue<string>("KAFKA_CATALOG_TOPIC")!);

            options.IntegrationEventFactory = IntegrationEventFactory<ProductCreatedIntegrationEvent>.Instance;
        });

        return builder;
    }

}
