using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Infrastructure;

namespace BFF.ProductCatalogService.Infrastructure.UoW;
public interface IUnitOfWork
{
    ProductCatalogDbContext DbContext { get; }
    ILogTailingOutboxMessageRepository OutboxForLogTailingRepository { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
