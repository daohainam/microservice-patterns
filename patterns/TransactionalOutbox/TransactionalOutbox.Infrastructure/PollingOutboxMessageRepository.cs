namespace TransactionalOutbox.Infrastructure;

public class PollingOutboxMessageRepository(PollingOutboxMessageRepositoryOptions options, OutboxDbContext dbContext) : IPollingOutboxMessageRepository
{
    public Task AddAsync(PollingOutboxMessage message)
    {
        dbContext.PollingOutboxMessages.Add(message);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<PollingOutboxMessage>> GetUnprocessedMessagesAsync()
    {
        return await dbContext.PollingOutboxMessages.Where(m => m.ProcessedDate == null && m.ProcessedCount < options.MaxRetries).ToListAsync();
    }

    public Task MarkAsFailedAsync(PollingOutboxMessage message, bool recoverable = true)
    {
        if (recoverable)
        {
            message.ProcessedCount++;
        }
        else
        {
            message.ProcessedCount = options.MaxRetries;
        }

        return Task.CompletedTask;
    }

    public Task MarkAsProcessedAsync(PollingOutboxMessage message)
    {
        message.ProcessedCount++;
        message.ProcessedDate = DateTime.UtcNow;

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}
