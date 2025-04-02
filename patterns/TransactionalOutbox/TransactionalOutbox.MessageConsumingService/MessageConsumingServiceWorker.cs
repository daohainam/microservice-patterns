using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace TransactionalOutbox.MessageConsumingService;

public class MessageConsumingServiceWorker(MessageConsumingServiceOptions options, ILogger<MessageConsumingServiceWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Connection to Kafka...");
            }

            var config = new ConsumerConfig
            {
                BootstrapServers = options.BootstrapServers,
                GroupId = options.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            logger.LogInformation("Trying to connect to {bs}...", config.BootstrapServers);

            await ConnectAndProcessMessagesAsync(config, [options.Topic], stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }

    public async Task ConnectAndProcessMessagesAsync(ConsumerConfig config, string[] topics, CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(config).Build();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Subcribing to {t}...", string.Join(", ", topics));
                consumer.Subscribe(topics);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(cancellationToken);

                    var message = consumeResult.Message;
                    if (message != null)
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation("Consumed message '{m}'", message.Value);
                        }

                        // Process the message
                        logger.LogInformation("Processing message '{m}'", message.Value);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Operation cancelled");
            }
            catch (ConsumeException e)
            {
                logger.LogError(e, "Error consuming message");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing message");
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }

        consumer.Close();
    }
}
