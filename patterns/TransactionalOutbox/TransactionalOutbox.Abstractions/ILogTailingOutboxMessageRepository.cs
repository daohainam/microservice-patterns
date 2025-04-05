namespace TransactionalOutbox.Abstractions;
public interface ILogTailingOutboxMessageRepository
{
    Task AddAsync(LogTailingOutboxMessage message);
    Task SaveChangesAsync();
}
