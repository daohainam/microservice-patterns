using MicroservicePatterns.DatabaseMigrationHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionalOutbox.Infrastructure.Data;
using TransactionalOutbox.Infrastructure.Service;

namespace TransactionalOutbox.Infrastructure;
public static class RegistrationExtensions
{
    public static void AddTransactionalOutbox(this IHostApplicationBuilder builder, string connectionStringName)
    {
        builder.AddNpgsqlDbContext<OutboxDbContext>(connectionStringName,
            configureDbContextOptions: options => {
                options.UseNpgsql(o => o.MigrationsHistoryTable("__OutboxMigrationsHistory"));
            }
            );

        builder.Services.AddHostedService<TransactionalOutboxService>();
    }

    public static Task MigrateOutboxDbContextAsync(this IHost host)
    {
        return host.MigrateDbContextAsync<OutboxDbContext>();
    }
}
