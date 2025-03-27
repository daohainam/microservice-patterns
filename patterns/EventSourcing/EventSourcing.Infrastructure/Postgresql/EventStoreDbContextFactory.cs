using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventSourcing.Infrastructure.Postgresql;
public class EventStoreDbContextFactory : IDesignTimeDbContextFactory<EventStoreDbContext>
{
    public EventStoreDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventStoreDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=EventStore;Username=postgres;Password=postgres");

        return new EventStoreDbContext(optionsBuilder.Options);
    }
}
