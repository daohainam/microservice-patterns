namespace Saga.OnlineStore.PaymentService;
public class ApiServices(
    PaymentDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public PaymentDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
