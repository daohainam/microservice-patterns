namespace EventSourcing.Banking.AccountService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddEventSourcing(Consts.DefaultDatabase);

        return builder;
    }
}
