namespace TransactionalOutbox.Abstractions;
public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message);
    Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync();
    Task MarkAsProcessedAsync(OutboxMessage message);
    Task MarkAsFailedAsync(OutboxMessage message);
}
