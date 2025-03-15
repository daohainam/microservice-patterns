using EventBus.Events;

namespace Saga.TripPlanner.IntegrationEvents;
public class HotelRoomBookedIntegrationEvent: IntegrationEvent
{
    public Guid RoomId { get; set; }
    public Guid TripId { get; set; }
}
public class HotelRoomBookingPendingIntegrationEvent : IntegrationEvent
{
    public Guid RoomId { get; set; }
    public Guid TripId { get; set; }
}
public class HotelRoomBookingRejectedIntegrationEvent : IntegrationEvent
{
    public Guid RoomId { get; set; }
    public Guid TripId { get; set; }
}
