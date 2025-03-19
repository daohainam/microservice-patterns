using Saga.TripPlanner.IntegrationEvents;

namespace Saga.TripPlanner.TicketService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<TicketDbContext>(Consts.DefaultDatabase);

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

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });

        var eventConsumingTopics = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics);
        if (!string.IsNullOrEmpty(eventConsumingTopics))
        {
            builder.AddKafkaEventConsumer(options => {
                options.ServiceName = "TripPlanningTicketService";
                options.KafkaGroupId = "saga-tripplanning-ticket-service";
                options.Topics.AddRange(eventConsumingTopics.Split(','));
                options.IntegrationEventFactory = IntegrationEventFactory<HotelRoomBookedIntegrationEvent>.Instance;
                options.AcceptEvent = e => e.IsEvent<HotelRoomBookingPendingIntegrationEvent, TripRejectedIntegrationEvent>();
            });
        }

        return builder;
    }
}
