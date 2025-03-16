namespace Saga.TripPlanner.PaymentService.Infrastructure.Data;
public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Card> Cards { get; set; } = default!;
}
