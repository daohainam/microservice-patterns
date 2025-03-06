using MicroservicePatterns.Shared;

namespace CQRS.Library.BorrowerService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowerDbContext>(Consts.DefaultDatabase);
        builder.AddKafkaEventPublisher("kafka");
        builder.Services.AddKafkaEventPublisher("cqrs-library-borrower");

        return builder;
    }
}
