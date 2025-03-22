using Saga.TripPlanner.IntegrationEvents;

namespace Saga.TripPlanner.TripPlanningService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<TripPlanningDbContext>(Consts.DefaultDatabase);

        builder.AddSagaClientServices();

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

        return builder;
    }
    public static IHostApplicationBuilder AddSagaClientServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("hotel",
            static client => client.BaseAddress = new("https+http://hotel"));

        builder.Services.AddScoped<SagaServices>(services => {
            IHttpClientFactory httpClientFactory = services.GetRequiredService<IHttpClientFactory>();

            var s = new SagaServices(
                httpClientFactory.CreateClient("hotel")
            );

            return s;
        });


        //builder.Services.AddHttpClient("ticket-service", Projects.Saga_TripPlanner_TicketService>();
        //builder.Services.AddHttpClient(Projects.Saga_TripPlanner_PaymentService>();
        return builder;
    }
}
