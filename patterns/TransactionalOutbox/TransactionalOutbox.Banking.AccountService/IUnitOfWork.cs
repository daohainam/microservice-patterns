using TransactionalOutbox.Banking.AccountService.Infrastructure.Data;
using TransactionalOutbox.Infrastructure;

namespace TransactionalOutbox.Banking.AccountService;
public interface IUnitOfWork
{
    AccountDbContext AccountDbContext { get; }
    OutboxMessageRepository OutboxMessageRepository { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
