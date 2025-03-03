using Confluent.Kafka;
using CQRS.Library.BorrowingHistoryApi.EventHandling;
using EventBus.Abstractions;
using System.Text.Json;

namespace CQRS.Library.BorrowingHistoryApi.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowingHistoryDbContext>("cqrs-borrowing-history-db");
        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
        builder.AddEventConsumer();

        return builder;
    }

    private static IHostApplicationBuilder AddEventConsumer(this IHostApplicationBuilder builder)
    {
        builder.AddKafkaMessageEnvelopConsumer("cqrs-library");
        builder.Services.AddSingleton(new EventHandlingWorkerOptions());
        builder.Services.AddSingleton<IIntegrationEventFactory, IntegrationEventFactory>();
        builder.Services.AddHostedService<EventHandlingService>();
        return builder;
    }
}

