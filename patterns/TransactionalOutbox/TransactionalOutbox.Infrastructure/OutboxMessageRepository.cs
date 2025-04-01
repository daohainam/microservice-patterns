using Microsoft.EntityFrameworkCore;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Infrastructure.Data;

namespace TransactionalOutbox.Infrastructure
{
    public class OutboxMessageRepository(OutboxMessageRepositoryOptions options, OutboxDbContext dbContext) : IOutboxMessageRepository
    {
        public Task AddAsync(OutboxMessage message)
        {
            dbContext.OutboxMessages.Add(message);

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync()
        {
            return await dbContext.OutboxMessages.Where(m => m.ProcessedDate == null && m.ProcessedCount < options.MaxRetries).ToListAsync();
        }

        public Task MarkAsFailedAsync(OutboxMessage message, bool recoverable = true)
        {
            if (recoverable)
            {
                message.ProcessedCount++;
            }
            else
            {
                message.ProcessedCount = options.MaxRetries;
            }

            return Task.CompletedTask;
        }

        public Task MarkAsProcessedAsync(OutboxMessage message)
        {
            message.ProcessedCount++;
            message.ProcessedDate = DateTime.UtcNow;

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }
    }
}
