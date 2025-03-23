
using Saga.TripPlanner.HotelService.Infrastructure.Entity;

namespace Saga.TripPlanner.HotelService.Apis;
public static class HotelApiExtensions
{
    public static IEndpointRouteBuilder MapHotelApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/saga/v1")
              .MapHotelApi()
              .WithTags("Hotel Api");

        return builder;
    }

    public static RouteGroupBuilder MapHotelApi(this RouteGroupBuilder group)
    {
        group.MapGet("rooms", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.Rooms.ToListAsync();
        });

        group.MapGet("rooms/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.Rooms.FindAsync(id);
        });

        group.MapPost("rooms", HotelApi.CreateRoom);

        group.MapPost("bookings", HotelApi.CreateBooking);
        group.MapPut("bookings/{bookingId:guid}/cancel", HotelApi.CancelBooking);


        return group;
    }
}
public class HotelApi
{ 
    internal static async Task<Results<Ok<Room>, BadRequest>> CreateRoom([AsParameters] ApiServices services, Room room)
    {
        if (room == null) {
            return TypedResults.BadRequest();
        }

        if (room.Price <= 0 || room.MaxOccupancy <= 0)
        {
            return TypedResults.BadRequest();
        }

        if (room.Id == Guid.Empty)
            room.Id = Guid.CreateVersion7();

        await services.DbContext.Rooms.AddAsync(room);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(room);
    }

    internal static async Task<Results<Ok<List<Booking>>, BadRequest, NotFound>> CreateBooking([AsParameters] ApiServices services, List<Booking> bookings)
    {
        foreach (var booking in bookings)
        {
            if (booking.RoomId == Guid.Empty)
            {
                return TypedResults.BadRequest();
            }

            var room = await services.DbContext.Rooms.FindAsync(booking.RoomId);
            if (room == null)
            {
                return TypedResults.NotFound();
            }

            // we need to make sure that the room is available for the booking dates
            var duplicatedBookings = room.Bookings.Where(b => b.CheckInDate <= booking.CheckOutDate && b.CheckOutDate >= booking.CheckInDate).ToList();

            if (duplicatedBookings.Count != 0)
            {
                services.Logger.LogInformation("Room is not available for the booking dates");
                return TypedResults.BadRequest();
            }

            if (booking.Id == Guid.Empty)
                booking.Id = Guid.CreateVersion7();

            booking.RoomId = booking.RoomId;
            booking.BookingDate = DateTime.UtcNow;
            booking.Status = BookingStatus.Booked;
            services.DbContext.Add(booking);
        }

        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(bookings);
    }

    internal static async Task<Results<Ok, BadRequest, NotFound>> CancelBooking([AsParameters] ApiServices services, List<Guid> bookingIds)
    {

        foreach (var bookingId in bookingIds)
        {
            var booking = await services.DbContext.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return TypedResults.NotFound();
            }

            if (booking.Status != BookingStatus.Booked)
            {
                services.Logger.LogInformation("Booking {id} is not in booked state", booking.Id);
                return TypedResults.BadRequest();
            }

            booking.Status = BookingStatus.Cancelled;
        }
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }

}
