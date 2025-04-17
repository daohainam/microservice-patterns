using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TransactionalOutbox.Abstractions;

namespace TransactionalOutbox.Publisher.Polling;
public class PollingPublisher
{
    private readonly IEventPublisher eventPublisher;
    private readonly IPollingOutboxMessageRepository repository;
    private readonly ILogger<PollingPublisher> logger;
    private readonly PollingPublisherOptions options = new();

    public PollingPublisher(IEventPublisher eventPublisher, IPollingOutboxMessageRepository outboxMessageRepository, ILogger<PollingPublisher> logger, Action<PollingPublisherOptions>? configureOptions = null)
    {
        this.eventPublisher = eventPublisher;
        this.repository = outboxMessageRepository;
        this.logger = logger;

        configureOptions?.Invoke(options);
    }

    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var messages = await repository.GetUnprocessedMessagesAsync();

            foreach (var message in messages)
            {
                try
                {
                    var @event = RebuildEvent(message);

                    if (@event == null)
                    {
                        logger.LogWarning("Failed to rebuild event from message {MessageId}", message.Id);
                        await repository.MarkAsFailedAsync(message, false); // mark as failed without retry
                        continue;
                    }

                    logger.LogInformation("Publish event from Polling publisher: {m}", message.Payload);
                    await eventPublisher.PublishAsync(@event);
                    await repository.MarkAsProcessedAsync(message);
                    await repository.SaveChangesAsync();

                    logger.LogInformation("Published message {MessageId}", message.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to publish message {MessageId}", message.Id);
                    await repository.MarkAsFailedAsync(message);
                }
            }

            if (!messages.Any() && !cancellationToken.IsCancellationRequested)
                await Task.Delay(5000, cancellationToken);
        }
    }
    private IntegrationEvent? RebuildEvent(PollingOutboxMessage message)
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
