
using Saga.TripPlanner.IntegrationEvents;

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
        trip.CreationDate = DateTime.UtcNow;

        await services.DbContext.Trips.AddAsync(trip);
        foreach (var hotelRoom in trip.HotelRoomBookings)
        {
            hotelRoom.Id = Guid.CreateVersion7();
            hotelRoom.BookingDate = trip.CreationDate;
        }

        await services.DbContext.SaveChangesAsync();

        await services.EventPublisher.PublishAsync(new TripCreatedIntegrationEvent()
        {
            TripId = trip.Id,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            CreationDate = trip.CreationDate,
            Status = TripStatus.Pending,
            TripName = trip.Name, 
            HotelRooms = [.. trip.HotelRoomBookings.Select(h => new TripHotelRoom()
            {
                Id = Guid.CreateVersion7(),
                RoomId = h.RoomId,
                BookingDate = h.BookingDate,
                CheckInDate = h.CheckInDate,
                CheckOutDate = h.CheckOutDate,
            })]
        });

        return TypedResults.Ok(trip);
    }
}
