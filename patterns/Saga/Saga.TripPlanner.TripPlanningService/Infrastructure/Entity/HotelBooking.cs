namespace Saga.TripPlanner.TripPlanningService.Infrastructure.Entity;

public class HotelBooking
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid TripId { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
}
