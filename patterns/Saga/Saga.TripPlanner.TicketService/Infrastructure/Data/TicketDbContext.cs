namespace Saga.TripPlanner.TicketService.Infrastructure.Data;
public class TicketDbContext(DbContextOptions<TicketDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets { get; set; } = default!;
    public DbSet<TicketType>  TicketTypes { get; set; } = default!;
}
