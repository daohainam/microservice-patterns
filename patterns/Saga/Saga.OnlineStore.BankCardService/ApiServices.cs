namespace Saga.OnlineStore.BankCardService;
public class ApiServices(
    BankCardDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public BankCardDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
