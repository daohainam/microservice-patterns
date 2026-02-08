using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Banking.AccountService.Infrastructure.Data;
using TransactionalOutbox.Infrastructure;

namespace TransactionalOutbox.Banking.AccountService;

/// <summary>
/// Defines a unit of work that coordinates database operations and transactional outbox messaging.
/// </summary>
/// <remarks>
/// This interface provides access to the account database context and outbox repositories,
/// ensuring that business operations and event publishing are coordinated in the same transaction.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Gets the database context for account-related operations.
    /// </summary>
    AccountDbContext AccountDbContext { get; }
    
    /// <summary>
    /// Gets the repository for polling-based outbox messages.
    /// </summary>
    IPollingOutboxMessageRepository OutboxForPollingRepository { get; }
    
    /// <summary>
    /// Gets the repository for log tailing-based outbox messages.
    /// </summary>
    ILogTailingOutboxMessageRepository OutboxForLogTailingRepository { get; }

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Commits the current transaction, persisting all changes.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rolls back the current transaction, discarding all changes.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
