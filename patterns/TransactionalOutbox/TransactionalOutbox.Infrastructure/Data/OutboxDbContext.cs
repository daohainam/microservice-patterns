using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using TransactionalOutbox.Abstractions;

namespace TransactionalOutbox.Infrastructure.Data;
public class OutboxDbContext: DbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = default!;
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>()
            .HasIndex(nameof(OutboxMessage.ProcessedDate), nameof(OutboxMessage.ProcessedCount), nameof(OutboxMessage.CreationDate));
    }
}
