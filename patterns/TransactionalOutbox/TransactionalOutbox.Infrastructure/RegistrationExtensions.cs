namespace TransactionalOutbox.Infrastructure;
public static class RegistrationExtensions
{
    private static readonly Assembly eventAssembly = typeof(AccountOpenedIntegrationEvent).Assembly;

    public static void AddTransactionalOutbox(this IHostApplicationBuilder builder, string connectionStringName)
    {
        builder.AddNpgsqlDbContext<OutboxDbContext>(connectionStringName,
            configureDbContextOptions: options => {
                options.UseNpgsql(o => o.MigrationsHistoryTable("__OutboxMigrationsHistory"));
            }
            );

        builder.Services.AddHostedService<TransactionalOutboxPollingService>();
        builder.Services.AddSingleton(new TransactionalOutboxLogTailingServiceOptions() { 
            PayloadTypeRsolver = (type) => eventAssembly.GetType(type) ?? throw new Exception($"Could not get type {type}"),
            ConnectionString = builder.Configuration.GetConnectionString(connectionStringName) ?? throw new Exception($"Connection string {connectionStringName} not found")
        });
        builder.Services.AddHostedService<TransactionalOutboxLogTailingService>();
    }

    public static Task MigrateOutboxDbContextAsync(this IHost host, Func<DatabaseFacade, CancellationToken?, Task>? postMigration = null, CancellationToken cancellationToken = default)
    {
        return host.MigrateDbContextAsync<OutboxDbContext>(postMigration: postMigration,
            cancellationToken: cancellationToken);
    }
}
