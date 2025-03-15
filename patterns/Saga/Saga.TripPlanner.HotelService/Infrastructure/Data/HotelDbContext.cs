namespace Saga.TripPlanner.HotelService.Infrastructure.Data;
public class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms { get; set; } = default!;
    public DbSet<Booking> Bookings { get; set; } = default!;
}
