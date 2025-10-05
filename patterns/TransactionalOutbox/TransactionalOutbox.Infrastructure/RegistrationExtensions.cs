namespace TransactionalOutbox.Infrastructure;
public static class RegistrationExtensions
{
    private static readonly Assembly eventAssembly = typeof(AccountOpenedIntegrationEvent).Assembly;

    public static void AddTransactionalOutbox(this IHostApplicationBuilder builder, string connectionStringName, Action<TransactionalOutboxOptions>? configureAction = null)
    {
        var options = new TransactionalOutboxOptions();
        configureAction?.Invoke(options);

        builder.AddNpgsqlDbContext<OutboxDbContext>(connectionStringName,
            configureDbContextOptions: options => {
                options.UseNpgsql(o => o.MigrationsHistoryTable("__OutboxMigrationsHistory"));
            }
            );

        if (options.PollingEnabled)
        {
            builder.Services.AddHostedService<TransactionalOutboxPollingService>();
        }
        if (options.LogTailingEnabled)
        {
            builder.Services.AddSingleton(new TransactionalOutboxLogTailingServiceOptions()
            {
                PayloadTypeRsolver = (type) => eventAssembly.GetType(type) ?? throw new Exception($"Could not get type {type}"),
                ConnectionString = builder.Configuration.GetConnectionString(connectionStringName) ?? throw new Exception($"Connection string {connectionStringName} not found")
            });
            builder.Services.AddHostedService<TransactionalOutboxLogTailingService>();
        }
    }

    public static Task MigrateOutboxDbContextAsync(this IHost host, Func<DatabaseFacade, CancellationToken?, Task>? postMigration = null, CancellationToken cancellationToken = default)
    {
        return host.MigrateDbContextAsync<OutboxDbContext>(postMigration: postMigration,
            cancellationToken: cancellationToken);
    }
}

public class TransactionalOutboxOptions
{
    //public int PollingIntervalInSeconds { get; set; } = 5;
    //public int BatchSize { get; set; } = 20;
    public bool PollingEnabled { get; set; } = true;
    public bool LogTailingEnabled { get; set; } = true;
}
