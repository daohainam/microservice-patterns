
using Saga.TripPlanner.HotelService.Apis;
using Saga.TripPlanner.HotelService.Infrastructure.Entity;
using Saga.TripPlanner.IntegrationEvents;
using Saga.TripPlanner.PaymentService.Apis;
using Saga.TripPlanner.TicketService.Infrastructure.Entity;

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
    internal static async Task<Results<Ok<Trip>, BadRequest>> CreateTrip([AsParameters] ApiServices services, SagaServices sagaServices, Trip trip)
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

        // we commmit the transaction here to make sure that the trip is created before we handle the saga
        await services.DbContext.SaveChangesAsync();

        await HandleSaga(services, sagaServices, trip);

        await services.EventPublisher.PublishAsync(new TripCreatedIntegrationEvent()
        {
            TripId = trip.Id,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            CreationDate = trip.CreationDate,
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

    private static async Task HandleSaga(ApiServices services, SagaServices sagaServices, Trip trip, CancellationToken cancellationToken = default)
    {
        // it is better to offload this Saga handing part to an async service, but I don't want to make this sample too complicated

        while (trip.Status != TripStatus.Rejected && trip.Status != TripStatus.Confirmed)
        {
            if (trip.Status == TripStatus.Pending)
            {
                var tickets = trip.TicketBookings.Select(t => new Ticket()
                {
                    Id = Guid.CreateVersion7(),
                    TicketTypeId = t.TicketTypeId
                });

                var ticketResponse = await sagaServices.TicketHttpClient.PostAsJsonAsync("/api/saga/v1/tickets",
                    tickets,
                    cancellationToken);

                trip.Status = TripStatus.TicketsBooked;
                await services.DbContext.SaveChangesAsync(cancellationToken);
            }
            else if (trip.Status == TripStatus.TicketsBooked)
            {
                var bookings = trip.HotelRoomBookings.Select(r => new Booking()
                {
                    RoomId = r.RoomId,
                    TripId = r.TripId,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                });

                var hotelRoomResponse = await sagaServices.HotelHttpClient.PostAsJsonAsync("/api/saga/v1/bookings", bookings, cancellationToken: cancellationToken);
                trip.Status = TripStatus.HotelRoomsBooked;
                await services.DbContext.SaveChangesAsync(cancellationToken);
            }
            else if (trip.Status == TripStatus.HotelRoomsBooked)
            {
                var payment = new CreditCardPayment()
                {
                    //CardHolderName = trip.

                };

                var paymentResponse = await sagaServices.PaymentHttpClient.PostAsJsonAsync("/api/saga/v1/bookings", payment, cancellationToken: cancellationToken);
                trip.Status = TripStatus.HotelRoomsBooked;
                await services.DbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
