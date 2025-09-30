namespace BFF.ProductCatalogService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<ProductCatalogDbContext>(Consts.DefaultDatabase);

        return builder;
    }
}
