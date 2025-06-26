namespace Saga.TripPlanner.PaymentService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<PaymentDbContext>(Consts.DefaultDatabase);

        builder.Services.AddMediator(cfg => {
            cfg.ServiceAssemblies.Add(typeof(Program).Assembly);
        });

        return builder;
    }
}
