namespace Saga.TripPlanner.HotelService;
public class ApiServices(
    HotelDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<HotelApi> logger)
{
    public HotelDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;
    public ILogger<HotelApi> Logger { get; } = logger;
}
