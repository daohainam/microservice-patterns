namespace Saga.TripPlanner.TripPlanningService.Infrastructure.Data;
public class TripPlanningDbContext(DbContextOptions<TripPlanningDbContext> options) : DbContext(options)
{
    public DbSet<Trip> Trips{ get; set; } = default!;
    public DbSet<HotelBooking> HotelBookings { get; set; } = default!;
    public DbSet<TicketBooking> Tickets { get; set; } = default!;
}
