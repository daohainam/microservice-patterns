using CQRS.Library.BorrowingService.Infrastructure.Data;
using MicroservicePatterns.Shared;

namespace CQRS.Library.BorrowingService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BorrowingDbContext>(Consts.DefaultDatabase);
        builder.AddKafkaEventPublisher("kafka");
        builder.Services.AddKafkaEventPublisher("cqrs-library-borrowing");

        return builder;
    }
}
