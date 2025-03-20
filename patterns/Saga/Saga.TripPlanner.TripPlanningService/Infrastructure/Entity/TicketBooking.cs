namespace Saga.TripPlanner.TripPlanningService.Infrastructure.Entity;
public class TicketBooking
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public DateTime BookingDate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
}

public enum BookingStatus
{
    Pending,
    Rejected,
    Confirmed
}