using EventSourcing.Infrastructure.Models;
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

        if (events == null || !events.Any())
        {
            return default;
        }

        var instance = Activator.CreateInstance<T>();
        if (instance == null)
        {
            return default;
        }

        var applyMethod = typeof(T).GetMethod("Apply", BindingFlags.Instance | BindingFlags.Public);
        if (applyMethod == null)
        {
            throw new InvalidOperationException($"Method 'Apply' not found in type '{typeof(T).Name}'.");
        }

        foreach (var eventData in events)
        {
            var type = (typeResolver != null ? typeResolver(eventData.Type) : Type.GetType(eventData.Type)) ?? throw new InvalidOperationException($"Type '{eventData.Type}' not found.");
            if (type.IsAssignableTo(typeof(Event)))
            {
                throw new InvalidOperationException($"Type '{type.Name}' is not assignable from '{nameof(Event)}'.");
            }

            var evt = JsonSerializer.Deserialize(eventData.Data, type);

            applyMethod.Invoke(instance, [evt]);
        }

        return instance;
    }
}
