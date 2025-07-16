using EventSourcing.Infrastructure.Models;
using EventSourcing.SeedWork;
using System.Reflection;
using System.Text.Json;

namespace EventSourcing.Infrastructure;
public static class EventStoreExtensions
{
    public static async Task<T?> FindAsync<T>(this IEventStore eventStore, 
        Guid streamId, 
        long? afterVersion = null, 
        Func<string, Type>? typeResolver = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var events = await eventStore.ReadAsync(streamId, afterVersion, cancellationToken: cancellationToken);

        if (events == null || events.Count == 0)
        {
            return default;
        }

        var instance = Activator.CreateInstance<T>();
        if (instance == null)
        {
            return default;
        }

        foreach (var eventData in events)
        {
            var type = (typeResolver != null ? typeResolver(eventData.Type) : Type.GetType(eventData.Type)) ?? throw new InvalidOperationException($"Type '{eventData.Type}' not found.");
            if (type.IsAssignableTo(typeof(Event)))
            {
                throw new InvalidOperationException($"Type '{type.Name}' is not assignable from '{nameof(Event)}'.");
            }

            var evt = JsonSerializer.Deserialize(eventData.Data, type);

            var applyMethod = typeof(T).GetMethod("Apply", BindingFlags.Instance | BindingFlags.Public, [ type ]) ?? throw new InvalidOperationException($"Method 'Apply' not found in type '{typeof(T).Name}'.");
            applyMethod.Invoke(instance, [evt]);
        }

        return instance;
    }

    public static async Task AppendAsync(this IEventStore eventStore,
        Aggregate aggregate,
        StreamStates streamState = StreamStates.Existing,
        CancellationToken cancellationToken = default)
    {
        var events = new List<Event>();
        foreach (var evt in aggregate.PendingChanges)
        {
            events.Add(new Event
            {
                Id = evt.EventId,
                StreamId = aggregate.Id,
                Data = JsonSerializer.Serialize(evt, evt.GetType()),
                Type = evt.GetType().FullName ?? throw new Exception($"Could not get fullname of type {evt.GetType()}"),
                CreatedAtUtc = evt.TimestampUtc
            });
        }

        await eventStore.AppendAsync(aggregate.Id, streamState, events, cancellationToken);
    }
}
