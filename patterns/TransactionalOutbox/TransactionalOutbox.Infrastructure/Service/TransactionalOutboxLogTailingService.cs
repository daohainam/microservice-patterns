using EventBus.Abstractions;
using EventBus.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Reflection;
using System.Text.Json;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Infrastructure.Data;
using TransactionalOutbox.IntegrationEvents;
using TransactionalOutbox.Publisher.Polling;

namespace TransactionalOutbox.Infrastructure.Service;

internal class TransactionalOutboxLogTailingService : BackgroundService
{
    // this is not a real log tailing service, which uses Postgresql WAL, but a notification handler

    private readonly TransactionalOutboxLogTailingServiceOptions options;
    private readonly IEventPublisher eventPublisher;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<TransactionalOutboxLogTailingService> logger;

    private static readonly Assembly eventAssembly = typeof(AccountOpenedIntegrationEvent).Assembly; 

    public TransactionalOutboxLogTailingService(TransactionalOutboxLogTailingServiceOptions options, IEventPublisher eventPublisher, IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory)
    {
        this.options = options;
        this.eventPublisher = eventPublisher;
        this.serviceScopeFactory = serviceScopeFactory; 
        this.logger = loggerFactory.CreateLogger<TransactionalOutboxLogTailingService>();
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting TransactionOutbox log tailing service...");

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var conn = new NpgsqlConnection(options.ConnectionString);

                await conn.OpenAsync(stoppingToken);
                conn.Notification += async (c, e) => {
                    if ("outbox_channel" == e.Channel)
                    {
                        var message = JsonSerializer.Deserialize<LogTailingOutboxMessage>(e.Payload);

                        if (message != null)
                        {
                            var @event = RebuildEvent(message);

                            if (@event == null)
                            {
                                logger.LogWarning("Failed to rebuild event from message {MessageId}", message.Id);
                            }
                            else
                            {
                                logger.LogInformation("Publish event from Log Tailing (using Notification) publisher: {m}", message.Payload);
                                await eventPublisher.PublishAsync(@event);
                            }
                        }
                    }
                };

                using (var cmd = new NpgsqlCommand("LISTEN outbox_channel", conn))
                {
                    await cmd.ExecuteNonQueryAsync(stoppingToken);
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    conn.Wait(); 
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing");
            }
        }
    }

    private IntegrationEvent? RebuildEvent(LogTailingOutboxMessage message)
    {
        var type = options.PayloadTypeRsolver(message.PayloadType);

        if (type == null)
        {
            logger.LogWarning("Failed to find type {PayloadType}", message.PayloadType);
            return null;
        }

        return JsonSerializer.Deserialize(message.Payload, type) as IntegrationEvent;
    }

}

public class TransactionalOutboxLogTailingServiceOptions
{
    public Func<string, Type?> PayloadTypeRsolver { get; set; } = Type.GetType; 
    public required string ConnectionString { get; set; } 
}