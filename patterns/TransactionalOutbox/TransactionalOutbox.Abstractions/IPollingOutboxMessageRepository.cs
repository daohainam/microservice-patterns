namespace TransactionalOutbox.Abstractions;

/// <summary>
/// Defines a repository for managing outbox messages in a polling-based transactional outbox pattern.
/// </summary>
/// <remarks>
/// This repository is used to store messages that need to be published to an event bus
/// in the same database transaction as the business operation. A background polling service
/// periodically retrieves unprocessed messages and publishes them.
/// </remarks>
public interface IPollingOutboxMessageRepository
{
    /// <summary>
    /// Adds a new outbox message to be processed later.
    /// </summary>
    /// <param name="message">The message to add to the outbox.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(PollingOutboxMessage message);
    
    /// <summary>
    /// Retrieves all unprocessed messages from the outbox.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing the collection of unprocessed messages.</returns>
    Task<IEnumerable<PollingOutboxMessage>> GetUnprocessedMessagesAsync();
    
    /// <summary>
    /// Marks a message as successfully processed.
    /// </summary>
    /// <param name="message">The message to mark as processed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MarkAsProcessedAsync(PollingOutboxMessage message);
    
    /// <summary>
    /// Marks a message as failed, optionally marking it as recoverable for retry.
    /// </summary>
    /// <param name="message">The message that failed to process.</param>
    /// <param name="recoverable">If true, the message can be retried; otherwise, it's permanently failed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MarkAsFailedAsync(PollingOutboxMessage message, bool recoverable = true);
    
    /// <summary>
    /// Persists all changes made to the outbox messages.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveChangesAsync();

}
