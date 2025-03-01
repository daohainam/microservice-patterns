namespace CQRS.Library.BorrowerApi.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowerDbContext>("cqrs-borrower");
        builder.AddKafkaEventPublisher("kafka");

        return builder;
    }
}
