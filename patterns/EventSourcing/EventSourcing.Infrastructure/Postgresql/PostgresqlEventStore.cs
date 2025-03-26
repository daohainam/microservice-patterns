using EventSourcing.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Infrastructure.Postgresql;
internal class PostgresqlEventStore : IEventStore
{
    private readonly EventStoreDbContext dbContext;

    public PostgresqlEventStore(EventStoreDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<long> AppendAsync(Guid streamId, StreamStates state, IEnumerable<Event> events, CancellationToken cancellationToken = default)
    {
        if (events == null || !events.Any())
        {
            return -1;
        }

        EventStream? stream;

        if (state == StreamStates.New)
        {
            stream = await dbContext.EventStreams.FindAsync(new object?[] { streamId }, cancellationToken: cancellationToken);
            if (stream != null)
            {
                throw new InvalidOperationException($"Stream '{streamId}' already exists.");
            }
        }
        else
        {
            stream = await dbContext.EventStreams.FindAsync(new object?[] { streamId }, cancellationToken: cancellationToken) ?? throw new InvalidOperationException($"Stream '{streamId}' not found.");
        }

        var lastId = Guid.Empty;

        foreach (var evt in events)
        {
            lastId = evt.Id;
            dbContext.Events.Add(new Event
            {
                Id = lastId,
                StreamId = streamId,
                Data = evt.Data,
                Type = evt.Type,
                CreatedAtUtc = evt.CreatedAtUtc,
                Metadata = evt.Metadata,
                // Version = auto increment
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return dbContext.Events.Where(evt => evt.Id == lastId).Single().Version;

    }

    public Task<IEnumerable<Event>> ReadAsync(Guid streamId, long? afterVersion = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
