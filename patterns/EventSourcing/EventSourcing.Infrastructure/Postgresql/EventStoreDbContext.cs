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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseIdentityByDefaultColumns(); // or we can use HiLo
        modelBuilder.Entity<Event>().Property(b => b.Version).HasDefaultValue();
    }
}
