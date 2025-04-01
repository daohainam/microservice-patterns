namespace TransactionalOutbox.Banking.AccountService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();

        builder.AddNpgsqlDbContext<AccountDbContext>(Consts.DefaultDatabase);
        builder.AddNpgsqlDbContext<OutboxDbContext>($"{Consts.DefaultDatabase}-OutBox");

        if (builder.Configuration.GetConnectionString(Consts.DefaultDatabase) is string connectionString
            && builder.Configuration.GetConnectionString($"{Consts.DefaultDatabase}-OutBox") is string outboxConnectionString)
        {
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(sp =>
            {
                var connection = new NpgsqlConnection(connectionString);

                return new UnitOfWork(connection);
            });
        }
        else
        {
            throw new InvalidOperationException($"Connection string '{Consts.DefaultDatabase}' not found.");
        }

        return builder;
    }
}
