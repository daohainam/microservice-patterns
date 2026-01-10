namespace Mcp.Saga.TripPlanner.McpServer;

public interface ITripPlanningService
{
    Task<IEnumerable<Trip>> GetTrips(CancellationToken cancellationToken = default);
    Task<Trip> GetTripById(Guid tripId, CancellationToken cancellationToken = default);
    Task<Trip> CreateTrip(Trip trip, CancellationToken cancellationToken = default);
}
