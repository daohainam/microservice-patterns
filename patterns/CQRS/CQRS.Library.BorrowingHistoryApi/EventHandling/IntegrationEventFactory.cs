using System.Text.Json;

namespace CQRS.Library.BorrowingHistoryApi.EventHandling
{
    public class IntegrationEventFactory : IIntegrationEventFactory
    {
        public IntegrationEvent? CreateEvent(string typeName, string value)
        {
            var t = Type.GetType(typeName) ?? throw new ArgumentException($"Type {typeName} not found");

            return JsonSerializer.Deserialize(value, t) as IntegrationEvent;
        }
    }
}
