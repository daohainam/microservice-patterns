using System.Text.Json.Serialization;

namespace Saga.TripPlanner.TripPlanningService.Infrastructure.Entity;
public class Trip
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Name { get; set; } = default!;
    public TripStatus Status { get; set; } = TripStatus.Pending;
    public DateTime CreationDate { get; set; }
    public List<TicketBooking> TicketBookings { get; set; } = [];
    public List<HotelBooking> HotelRoomBookings { get; set; } = [];
    public string CardNumber { get; set; } = default!;
    public string CardHolderName { get; set; } = default!;
    public string ExpirationDate { get; set; } = default!;
    public string Cvv { get; set; } = default!;
    public decimal Amount { get; set; }
}

public enum TripStatus
{
 
    Pending,
    TicketsBooked,
    HotelRoomsBooked,
    Rejected,
    HotelRoomBookingCancelled,
    TicketsCancelled,
    PaymentFailed,
    Confirmed
}
