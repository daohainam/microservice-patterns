using EventBus.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TransactionalOutbox.Infrastructure.Data;
using TransactionalOutbox.IntegrationEvents;
using TransactionalOutbox.Publisher.Polling;

namespace TransactionalOutbox.Infrastructure.Service;

internal class TransactionalOutboxService : BackgroundService
{
    private readonly IEventPublisher eventPublisher;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<PollingPublisher> publisherLogger;
    private readonly ILogger<TransactionalOutboxService> logger;

    private static readonly Assembly eventAssembly = typeof(AccountOpenedIntegrationEvent).Assembly; 

    public TransactionalOutboxService(IEventPublisher eventPublisher, IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory)
    {
        this.eventPublisher = eventPublisher;
        this.serviceScopeFactory = serviceScopeFactory; 
        this.publisherLogger = loggerFactory.CreateLogger<PollingPublisher>();
        this.logger = loggerFactory.CreateLogger<TransactionalOutboxService>();
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting TransactionOutboxService...");

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

        var publisher = new PollingPublisher(
            this.eventPublisher,
            new OutboxMessageRepository(new OutboxMessageRepositoryOptions(), dbContext),
            publisherLogger,
            options => options.PayloadTypeRsolver = (type) => eventAssembly.GetType(type) ?? throw new Exception($"Could not get type {type}")
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await publisher.ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing TransactionOutboxService");
            }
        }
    }
}