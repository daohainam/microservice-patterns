namespace Saga.TripPlanner.HotelService.Infrastructure.Entity;
public class Booking
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid TripId { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Booked;
}

public enum BookingStatus
{
    Booked,
    Cancelled
}