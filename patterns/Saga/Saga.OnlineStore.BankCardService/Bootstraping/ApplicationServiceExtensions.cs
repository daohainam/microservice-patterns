using MicroservicePatterns.Shared;
using Saga.OnlineStore.BankCardService.Infrastructure.Data;

namespace Saga.OnlineStore.BankCardService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BankCardDbContext>(Consts.DefaultDatabase);
        builder.AddKafkaEventPublisher("kafka");
        builder.Services.AddKafkaEventPublisher(builder.Configuration.GetValue<string>(Consts.EnvKafkaTopic));

        return builder;
    }
}
