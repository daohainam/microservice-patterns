namespace CQRS.Library.BorrowingHistoryApi.Bootstraping;
public static class DbContextMigrationExtensions
{
    public static async Task<IHost> MigrateDbContextAsync<TContext>(this IHost host, CancellationToken cancellationToken = default) where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var context = services.GetService<TContext>();
        if (context is not null)
        {
            try
            {
                var dbCreator = context.GetService<IRelationalDatabaseCreator>();

                var strategy = context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    if (!await dbCreator.ExistsAsync(cancellationToken))
                    {
                        logger.LogInformation("Creating database associated with context {DbContextName}", typeof(TContext).Name);
                        await dbCreator.CreateAsync(cancellationToken);
                    }
                });


                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);
                await strategy.ExecuteAsync(async () =>
                {
                    await context.Database.MigrateAsync(cancellationToken);
                });
                logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
            }
        }
        return host;
    }

    public static async Task<IHost> MigrateApiDbContextAsync(this IHost host, CancellationToken cancellationToken = default) 
    {
        await host.MigrateDbContextAsync<BorrowingHistoryDbContext>(cancellationToken: cancellationToken);
        return host;
    }
}
