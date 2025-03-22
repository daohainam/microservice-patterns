namespace Saga.TripPlanner.TripPlanningService.Infrastructure.Entity;
public class TicketBooking
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public string TicketTypeId { get; set; } = default!;
    public DateTime BookingDate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Booked;
}

public enum BookingStatus
{
    Booked,
    Canncelled,
}