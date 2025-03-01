namespace CQRS.Library.BorrowingHistoryApi.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowingDbContext>("cqrs-borrowing-history-db");
        
        builder.AddEventConsumer();

        return builder;
    }

    private static IHostApplicationBuilder AddEventConsumer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<EventHandlingWorker>();
        return builder;
    }
}
