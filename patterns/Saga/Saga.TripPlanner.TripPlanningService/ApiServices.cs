namespace Saga.TripPlanner.TripPlanningService;
public class ApiServices(
    TripPlanningDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<TripPlanningApi> logger)
{
    public TripPlanningDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;
    public ILogger<TripPlanningApi> Logger { get; } = logger;
}
