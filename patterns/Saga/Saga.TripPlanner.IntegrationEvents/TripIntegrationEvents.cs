using EventBus.Events;

namespace Saga.TripPlanner.IntegrationEvents;
public class TripCreatedIntegrationEvent: IntegrationEvent
{
    public Guid TripId { get; set; }
    public string TripName { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Pending;
    public DateTime CreationDate { get; set; }
    public List<TripHotelRoom> HotelRooms { get; set; } = [];
}

public class TripBookedIntegrationEvent : TripCreatedIntegrationEvent
{
}

public class TripRejectedIntegrationEvent : TripCreatedIntegrationEvent
{
    public string Reason { get; set; } = default!;
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
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}

public class TripFlightTicket
{
    public string FlightNumber { get; set; } = default!;
    public DateTime FlightTime { get; set; } = default!;
    public Guid BookedTicketId { get; set; }
}
