using Microsoft.EntityFrameworkCore;
using TransactionalOutbox.Abstractions;

namespace TransactionalOutbox.Infrastructure.Data;
public class OutboxDbContext: DbContext
{
    public DbSet<OutboxMessage> OutboxItems { get; set; } = default!;
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options)
    {
    }
}
