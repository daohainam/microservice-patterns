using EventSourcing.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Infrastructure.Postgresql;

internal class EventStoreDbContext: DbContext
{
    public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<EventStream> EventStreams { get; set; }
}
