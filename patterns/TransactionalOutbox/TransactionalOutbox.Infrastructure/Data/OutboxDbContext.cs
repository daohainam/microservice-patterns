using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using TransactionalOutbox.Abstractions;

namespace TransactionalOutbox.Infrastructure.Data;
public class OutboxDbContext: DbContext
{
    public DbSet<PollingOutboxMessage> PollingOutboxMessages { get; set; } = default!;
    public DbSet<LogTailingOutboxMessage> LogTailingOutboxMessages { get; set; } = default!;
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PollingOutboxMessage>()
            .HasIndex(nameof(PollingOutboxMessage.ProcessedDate), nameof(PollingOutboxMessage.ProcessedCount), nameof(PollingOutboxMessage.CreationDate));
    }
}
