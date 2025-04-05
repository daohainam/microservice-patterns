namespace TransactionalOutbox.Abstractions;

public interface IPollingOutboxMessageRepository
{
    Task AddAsync(PollingOutboxMessage message);
    Task<IEnumerable<PollingOutboxMessage>> GetUnprocessedMessagesAsync();
    Task MarkAsProcessedAsync(PollingOutboxMessage message);
    Task MarkAsFailedAsync(PollingOutboxMessage message, bool recoverable = true);
    Task SaveChangesAsync();

}
