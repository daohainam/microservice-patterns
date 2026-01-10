namespace Mcp.Saga.TripPlanner.McpServer;

public class TripPlanningService(HttpClient tripPlanningHttpClient) : ITripPlanningService
{
    public async Task<IEnumerable<Trip>> GetTrips(CancellationToken cancellationToken = default)
    {
        var trips = await tripPlanningHttpClient.GetFromJsonAsync<IEnumerable<Trip>>("/api/saga/v1/trips", cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve trips from the trip planning service.");
        return trips;
    }

    public async Task<Trip> GetTripById(Guid tripId, CancellationToken cancellationToken = default)
    {
        var trip = await tripPlanningHttpClient.GetFromJsonAsync<Trip>($"/api/saga/v1/trips/{tripId}", cancellationToken: cancellationToken) ?? throw new InvalidOperationException($"Failed to retrieve trip {tripId}");
        return trip;
    }

    public async Task<Trip> CreateTrip(Trip trip, CancellationToken cancellationToken = default)
    {
        var response = await tripPlanningHttpClient.PostAsJsonAsync("/api/saga/v1/trips", trip, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        var createdTrip = await response.Content.ReadFromJsonAsync<Trip>(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to create trip.");
        return createdTrip;
    }
}
