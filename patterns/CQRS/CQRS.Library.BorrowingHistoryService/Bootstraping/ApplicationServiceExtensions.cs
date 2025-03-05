namespace CQRS.Library.BorrowingHistoryService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowingHistoryDbContext>("cqrs-library-borrowing-history-db");
        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
        builder.AddKafkaEventConsumer(options => {
            options.Topics.Add("cqrs-library-book");
            options.Topics.Add("cqrs-library-borrower");
            options.Topics.Add("cqrs-library-borrowing");

            options.IntegrationEventFactory = IntegrationEventFactory<BookCreatedIntegrationEvent>.Instance;
        });

        return builder;
    }
}

