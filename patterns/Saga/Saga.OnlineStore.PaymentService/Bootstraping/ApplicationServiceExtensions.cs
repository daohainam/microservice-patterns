namespace Saga.OnlineStore.PaymentService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<PaymentDbContext>(Consts.DefaultDatabase);

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });

        builder.AddKafkaProducer("kafka");
        var kafkaTopic = builder.Configuration.GetValue<string>(Consts.Env_EventPublishingTopics);
        if (!string.IsNullOrEmpty(kafkaTopic))
        {
            builder.AddKafkaEventPublisher(kafkaTopic);
        }
        else
        {
            builder.Services.AddTransient<IEventPublisher, NullEventPublisher>();
        }

        var eventConsumingTopics = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics);
        if (!string.IsNullOrEmpty(eventConsumingTopics))
        {
            builder.AddKafkaEventConsumer(options => {
                options.ServiceName = "OnlineStorePaymentService";
                options.KafkaGroupId = "saga-onlinestore-payment-service";
                options.Topics.AddRange(eventConsumingTopics.Split(','));
                options.IntegrationEventFactory = IntegrationEventFactory<ProductCreatedIntegrationEvent>.Instance;
                options.AcceptEvent = e => e.IsEvent<OrderItemsReservedIntegrationEvent>();
            });
        }

        return builder;
    }
}
