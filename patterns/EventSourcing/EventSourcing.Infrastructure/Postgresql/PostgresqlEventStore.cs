using EventSourcing.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Infrastructure.Postgresql;
internal class PostgresqlEventStore : IEventStore
{
    private readonly EventStoreDbContext dbContext;

    public PostgresqlEventStore(EventStoreDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Guid> AppendAsync(Guid streamId, StreamStates state, IEnumerable<Event> events)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Event>> ReadAsync(Guid streamId, long? afterVersion = null)
    {
        throw new NotImplementedException();
    }
}
