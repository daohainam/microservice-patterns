using Npgsql;
using System.Data;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Infrastructure.Data;

namespace BFF.ProductCatalogService.Infrastructure.UoW;

public class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
{
    public ProductCatalogDbContext DbContext => productCatalogDbContext ?? throw new Exception("ProductCatalogDbContext is not initialized.");

    public ILogTailingOutboxMessageRepository OutboxForLogTailingRepository => outboxForLogTailingRepository ?? throw new Exception("OutboxForLogTailingRepository is not initialized.");

    private readonly NpgsqlConnection connection;

    private NpgsqlTransaction? transaction; 
    private ProductCatalogDbContext? productCatalogDbContext;
    private OutboxDbContext? outboxDbContext;
    private LogTailingOutboxMessageRepository? outboxForLogTailingRepository;
    private bool disposedValue;

    public UnitOfWork(NpgsqlConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        this.connection = connection;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await connection.OpenAsync(cancellationToken);
            transaction = await connection.BeginTransactionAsync(cancellationToken);

            var accountDbContextOptions = new DbContextOptionsBuilder<ProductCatalogDbContext>()
             .UseNpgsql(connection)
             .Options;

            productCatalogDbContext = new ProductCatalogDbContext(accountDbContextOptions);
            await productCatalogDbContext.Database.UseTransactionAsync(transaction, cancellationToken: cancellationToken);

            var outboxDbContextOptions = new DbContextOptionsBuilder<OutboxDbContext>()
             .UseNpgsql(connection)
             .Options;

            outboxDbContext = new OutboxDbContext(outboxDbContextOptions);
            await outboxDbContext.Database.UseTransactionAsync(transaction, cancellationToken: cancellationToken);

            outboxForLogTailingRepository = new LogTailingOutboxMessageRepository(outboxDbContext);
        }
        catch
        {
            await connection.CloseAsync();
            throw;
        }
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction == null)
        {
            throw new InvalidOperationException("Transaction is not initialized.");
        }

        return transaction.CommitAsync(cancellationToken);
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction == null)
        {
            throw new InvalidOperationException("Transaction is not initialized.");
        }

        return transaction.RollbackAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }

                transaction?.Dispose();
                connection?.Dispose();
                productCatalogDbContext?.Dispose();
                outboxDbContext?.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!disposedValue)
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }

            if (productCatalogDbContext is not null)
            {
                await productCatalogDbContext.DisposeAsync();
            }

            if (outboxDbContext is not null)
            {
                await outboxDbContext.DisposeAsync();
            }

            if (connection is not null)
            {
                await connection.DisposeAsync();
            }

            disposedValue = true;
        }

        GC.SuppressFinalize(this);
    }
}
