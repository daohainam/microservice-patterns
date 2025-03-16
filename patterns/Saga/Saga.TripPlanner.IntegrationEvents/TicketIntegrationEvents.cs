using EventBus.Events;

namespace Saga.TripPlanner.IntegrationEvents;
public class TicketBookedIntegrationEvent: IntegrationEvent
{
    public Guid TicketId { get; set; }
}
public class TicketBookingRejectedIntegrationEvent : IntegrationEvent
{
    public Guid TicketId { get; set; }
}
