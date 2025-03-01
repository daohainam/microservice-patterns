namespace CQRS.Library.BorrowingApi.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowingDbContext>("cqrs-borrowing-db");
        builder.AddKafkaEventPublisher("kafka");
        builder.Services.AddKafkaEventPublisher("borrowing");

        return builder;
    }
}
