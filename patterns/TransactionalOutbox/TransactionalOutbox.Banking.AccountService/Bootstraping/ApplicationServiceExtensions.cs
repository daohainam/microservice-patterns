namespace TransactionalOutbox.Banking.AccountService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(sp =>
        {
            if (builder.Configuration.GetConnectionString(Consts.DefaultDatabase) is string connectionString)
            {
                var connection = new NpgsqlConnection(connectionString);

                return new UnitOfWork(connection);
            }
            else
            {
                throw new InvalidOperationException($"Connection string '{Consts.DefaultDatabase}' not found.");
            }
        });

        return builder;
    }
}
