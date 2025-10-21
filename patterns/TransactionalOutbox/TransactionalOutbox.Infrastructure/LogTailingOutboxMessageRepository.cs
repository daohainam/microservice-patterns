namespace TransactionalOutbox.Infrastructure;
public class LogTailingOutboxMessageRepository(OutboxDbContext dbContext) : ILogTailingOutboxMessageRepository
{
    public Task AddAsync(LogTailingOutboxMessage message)
    {
        dbContext.LogTailingOutboxMessages.Add(message);
        return Task.CompletedTask;
    }

    public static Task MarkAsProcessedAsync(PollingOutboxMessage message)
    {
        message.ProcessedCount++;
        message.ProcessedDate = DateTime.UtcNow;

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
