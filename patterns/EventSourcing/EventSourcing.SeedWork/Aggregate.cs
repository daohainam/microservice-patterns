using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventSourcing.SeedWork;
public abstract class Aggregate
{
    public Guid Id { get; set; }
    public long Version { get; set; }
    public IEnumerable<IDomainEvent> PendingChanges => _changes;

    [JsonIgnore]
    public long NextVersion => Version + 1;

    [JsonIgnore]
    protected readonly List<IDomainEvent> _changes = [];

    protected void Apply(IDomainEvent @event)
    {
        if (@event.Version != NextVersion)
        {
            throw new InvalidOperationException("Version mismatched");
        }

        ApplyChange(@event);

        _changes.Add(@event);
        Version = @event.Version; // => Version = NextVersion
    }

    protected void ApplyChange(IDomainEvent @event)
    {
        var method = this.GetType().GetMethod("Apply", [@event.GetType()]);
        if (method == null)
        {
            throw new InvalidOperationException($"Method Apply for {@event.GetType()} not found");
        }
        else
        {
            method.Invoke(this, [@event]);
        }
    }
}
