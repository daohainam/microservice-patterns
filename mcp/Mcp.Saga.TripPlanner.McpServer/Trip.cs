namespace Mcp.Saga.TripPlanner.McpServer;

public class Trip
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Name { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreationDate { get; set; }
    public List<TicketBooking> TicketBookings { get; set; } = [];
    public List<HotelBooking> HotelRoomBookings { get; set; } = [];
    public string CardNumber { get; set; } = default!;
    public string CardHolderName { get; set; } = default!;
    public string ExpirationDate { get; set; } = default!;
    public string Cvv { get; set; } = default!;
    public decimal Amount { get; set; }
}

public class TicketBooking
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Guid TicketTypeId { get; set; }
}

public class HotelBooking
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Guid RoomId { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}
