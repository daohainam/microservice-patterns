using EventBus.Events;

namespace Saga.TripPlanner.IntegrationEvents;
public class TripCreatedIntegrationEvent: IntegrationEvent
{
    public Guid TripId { get; set; }
    public string TripName { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Pending;
    public List<TripHotelRoom> HotelRooms { get; set; } = [];
}

public class TripBookedIntegrationEvent : TripCreatedIntegrationEvent
{
}

public enum TripStatus
{
    Pending,
    Rejected,
    Completed,
    Cancelled
}

public class TripHotelRoom
{
    public string RoomType { get; set; } = default!;
    public Guid BookedRoomId { get; set; }
}

public class TripFlightTicket
{
    public string FlightNumber { get; set; } = default!;
    public DateTime FlightTime { get; set; } = default!;
    public Guid BookedTicketId { get; set; }
}
