namespace Saga.TripPlanner.TicketService;
public class ApiServices(
    TicketDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<TicketApi> logger)
{
    public TicketDbContext DbContext => dbContext;
    public IEventPublisher EventPublisher => eventPublisher;
    public ILogger<TicketApi> Logger { get; } = logger;
}
