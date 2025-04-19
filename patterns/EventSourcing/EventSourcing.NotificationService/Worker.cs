using EventSourcing.Infrastructure.Models;
using Npgsql;
using System.Text.Json;

namespace EventSourcing.NotificationService;

public class Worker : BackgroundService
{
    private readonly NotificationServiceOptions options;
    private readonly ILogger<Worker> logger;

    public Worker(
        NotificationServiceOptions options,
        ILogger<Worker> logger)
    {
        this.options = options;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Waiting for events...");

                var conn = new NpgsqlConnection(options.ConnectionString);

                await conn.OpenAsync(stoppingToken);
                conn.Notification += (c, e) => {
                    if ("event_channel" == e.Channel)
                    {
                        var message = JsonSerializer.Deserialize<Event>(e.Payload);

                        if (message != null)
                        {
                            logger.LogInformation("Event received: {event}", message.Data);
                        }
                        else
                        {
                            logger.LogWarning("Event deserialization failed: {payload}", e.Payload);
                        }
                    }
                };

                using (var cmd = new NpgsqlCommand("LISTEN event_channel", conn))
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
}

public class NotificationServiceOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}