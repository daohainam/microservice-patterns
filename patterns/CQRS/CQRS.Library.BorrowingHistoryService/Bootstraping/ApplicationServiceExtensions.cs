using MicroservicePatterns.Shared;

namespace CQRS.Library.BorrowingHistoryService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowingHistoryDbContext>(Consts.DefaultDatabase);
        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });

        var topicNames = builder.Configuration.GetValue<string>("KAFKA_INCOMMING_TOPICS");
        if (!string.IsNullOrEmpty(topicNames))
        {
            var topics = topicNames.Split(',');

            builder.AddKafkaEventConsumer(options => {
                options.Topics.AddRange(topics);

                options.IntegrationEventFactory = IntegrationEventFactory<BookCreatedIntegrationEvent>.Instance;
            });
        }
        else
        {

        }

        return builder;
    }
}

