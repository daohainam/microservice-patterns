namespace Saga.TripPlanner.HotelService;
public class ApiServices(
    CatalogDbContext dbContext,
    IEventPublisher eventPublisher)
{
    public CatalogDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;

}
