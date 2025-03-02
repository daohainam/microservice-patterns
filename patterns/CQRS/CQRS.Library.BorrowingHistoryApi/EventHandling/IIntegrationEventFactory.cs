namespace CQRS.Library.BorrowingHistoryApi.EventHandling;
public interface IIntegrationEventFactory
{
    IntegrationEvent? CreateEvent(string typeName, string value);
}
