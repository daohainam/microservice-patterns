
namespace Saga.TripPlanner.TripPlanningService.Apis;
public static class TripPlanningApiExtensions
{
    public static IEndpointRouteBuilder MapHotelApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/saga/v1")
              .MapTripPlanningApi()
              .WithTags("Trip Planning Api");

        return builder;
    }

    public static RouteGroupBuilder MapTripPlanningApi(this RouteGroupBuilder group)
    {
        group.MapGet("trips", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.Trips.ToListAsync();
        });

        group.MapGet("trips/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.Trips.FindAsync(id);
        });

        group.MapPost("trips", TripPlanningApi.CreateTrip);

        return group;
    }
}
public class TripPlanningApi
{ 
    internal static async Task<Results<Ok<Trip>, BadRequest>> CreateTrip([AsParameters] ApiServices services, Trip trip)
    {
        if (trip == null) {
            return TypedResults.BadRequest();
        }

        if (trip.Id == Guid.Empty)
            trip.Id = Guid.CreateVersion7();

        await services.DbContext.Trips.AddAsync(trip);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(trip);
    }
}
