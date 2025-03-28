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
            // stream = await dbContext.EventStreams.FindAsync([streamId], cancellationToken: cancellationToken);
            // if (stream != null)
            // {
            //     throw new InvalidOperationException($"Stream '{streamId}' already exists.");
            // }

            // we don't need to check if the stream exists, we can just create it, if it doesn't exist then just throw an exception

            stream = new EventStream
            {
                Id = streamId, 
                CurrentVersion = 0
            };
            dbContext.EventStreams.Add(stream);
        }
        else
        {
            stream = await dbContext.EventStreams.FindAsync([streamId], cancellationToken: cancellationToken) ?? throw new InvalidOperationException($"Stream '{streamId}' not found.");
        }

        var lastId = Guid.Empty;

        foreach (var evt in events)
        {
            var @event = new Event
            {
                Id = lastId,
                StreamId = streamId,
                Data = evt.Data,
                Type = evt.Type,
                CreatedAtUtc = evt.CreatedAtUtc,
                Metadata = evt.Metadata ?? string.Empty,
                Version = evt.Version
            };

            dbContext.Events.Add(@event);

            stream.CurrentVersion = evt.Version;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return stream.CurrentVersion;
    }

    public Task<IEnumerable<Event>> ReadAsync(Guid streamId, long? afterVersion = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
