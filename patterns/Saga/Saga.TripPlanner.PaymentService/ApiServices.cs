namespace Saga.TripPlanner.PaymentService;
public class ApiServices(
    PaymentDbContext dbContext,
    ILogger<PaymentApi> logger)
{
    public PaymentDbContext DbContext => dbContext;
    public ILogger<PaymentApi> Logger => logger;
}
