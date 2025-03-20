namespace Saga.TripPlanner.PaymentService.Infrastructure.Data;
public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<CreditCard> CreditCards { get; set; } = default!;
}
