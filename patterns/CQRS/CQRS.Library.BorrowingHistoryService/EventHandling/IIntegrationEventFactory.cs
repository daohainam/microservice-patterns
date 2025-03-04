namespace CQRS.Library.BorrowingHistoryService.EventHandling;
public interface IIntegrationEventFactory
{
    IntegrationEvent? CreateEvent(string typeName, string value);
}
