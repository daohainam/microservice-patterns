namespace WebHook.DeliveryService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<DeliveryServiceDbContext>(Consts.DefaultDatabase, configureDbContextOptions: dbContextOptionsBuilder =>
        {
            dbContextOptionsBuilder.UseNpgsql(builder => builder.MigrationsAssembly(typeof(DeliveryServiceDbContext).Assembly.FullName));
        });
        builder.Services.AddSingleton<ISecretKeyService>(sp => new SecretKeyService(() => "12EDEB76-3EC7-49A3-9F5C-BBC50AB2B61F"));

        return builder;
    }
}
