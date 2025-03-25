using Aspire.Npgsql.EntityFrameworkCore.PostgreSQL;
using EventSourcing.Infrastructure.Postgresql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventSourcing.Infrastructure;
public static class Extensions
{
    public static IHostApplicationBuilder AddEventSourcing(this IHostApplicationBuilder builder, string connectionName)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Services.FirstOrDefault(sd => sd.ServiceType == typeof(DbContextOptions<EventStoreDbContext>)) != null) 
        {
            throw new InvalidOperationException("EventSourcing is already registered.");
        };

        if (builder.Configuration.GetConnectionString(connectionName) is string connectionString)
        {
            builder.Services.AddDbContextPool<EventStoreDbContext>((DbContextOptionsBuilder dbContextOptionsBuilder) =>
            {
                dbContextOptionsBuilder.UseNpgsql(connectionString);
            });

            builder.Services.AddScoped<IEventStore, PostgresqlEventStore>(sp =>
            {
                return new PostgresqlEventStore(sp.GetRequiredService<EventStoreDbContext>());
            });
        }
        else
        {
            throw new InvalidOperationException($"Connection string '{connectionName}' not found.");
        }

        return builder;
    }
}
