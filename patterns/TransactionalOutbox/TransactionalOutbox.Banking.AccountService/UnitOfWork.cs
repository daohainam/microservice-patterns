﻿using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using System.Diagnostics;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Banking.AccountService.Infrastructure.Data;
using TransactionalOutbox.Infrastructure;
using TransactionalOutbox.Infrastructure.Data;

namespace TransactionalOutbox.Banking.AccountService;
public class UnitOfWork : IUnitOfWork, IDisposable
{
    public AccountDbContext AccountDbContext => accountDbContext ?? throw new Exception("AccountDbContext is not initialized.");

    public IPollingOutboxMessageRepository OutboxForPollingRepository => outboxForPollingRepository ?? throw new Exception("OutboxForPollingRepository is not initialized.");
    public ILogTailingOutboxMessageRepository OutboxForLogTailingRepository => outboxForLogTailingRepository ?? throw new Exception("OutboxForLogTailingRepository is not initialized.");

    private readonly NpgsqlConnection connection;

    private NpgsqlTransaction? transaction; 
    private AccountDbContext? accountDbContext;
    private OutboxDbContext? outboxDbContext;
    private PollingOutboxMessageRepository? outboxForPollingRepository;
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

            var accountDbContextOptions = new DbContextOptionsBuilder<AccountDbContext>()
             .UseNpgsql(connection)
             .Options;

            accountDbContext = new AccountDbContext(accountDbContextOptions);
            await accountDbContext.Database.UseTransactionAsync(transaction, cancellationToken: cancellationToken);

            var outboxDbContextOptions = new DbContextOptionsBuilder<OutboxDbContext>()
             .UseNpgsql(connection)
             .Options;

            outboxDbContext = new OutboxDbContext(outboxDbContextOptions);
            await outboxDbContext.Database.UseTransactionAsync(transaction, cancellationToken: cancellationToken);

            outboxForPollingRepository = new PollingOutboxMessageRepository(new PollingOutboxMessageRepositoryOptions(), outboxDbContext);
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
                accountDbContext?.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
