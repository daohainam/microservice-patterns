namespace WebHook.DeliveryService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<DeliveryServiceDbContext>(Consts.DefaultDatabase);

        return builder;
    }
}
