using CQRS.Library.BorrowerService.Infrastructure.Data;

namespace CQRS.Library.BorrowerService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowerDbContext>("cqrs-borrower-db");
        builder.AddKafkaEventPublisher("kafka");
        builder.Services.AddKafkaEventPublisher("borrower");

        return builder;
    }
}
