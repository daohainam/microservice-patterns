using System.Text.Json.Serialization;

namespace Saga.TripPlanner.HotelService.Infrastructure.Entity;
public class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int MaxOccupancy { get; set; } = default!;
    [JsonIgnore]
    public List<Booking> Bookings { get; set; } = [];
}
