using EventBus.Events;

namespace Saga.TripPlanner.IntegrationEvents;
public class TripPaidIntegrationEvent : IntegrationEvent
{
    public Guid TripId { get; set; }
}

public class TripPaymentRejectedIntegrationEvent : IntegrationEvent
{
    public Guid TripId { get; set; }
}
