
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

        group.MapPost("rooms/{id:guid}/bookings", HotelApi.CreateBooking);
        group.MapPut("bookings/{bookingId:guid}/confirm", HotelApi.ConfirmBooking);


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

        if (room.Id == Guid.Empty)
            room.Id = Guid.CreateVersion7();

        await services.DbContext.Rooms.AddAsync(room);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(room);
    }

    internal static async Task<Results<Ok<Booking>, BadRequest, NotFound>> CreateBooking([AsParameters] ApiServices services, Guid id, Booking booking)
    {
        if (id == Guid.Empty)
        {
            return TypedResults.BadRequest();
        }

        var room = await services.DbContext.Rooms.FindAsync(id);
        if (room == null)
        {
            return TypedResults.NotFound();
        }

        // we need to make sure that the room is available for the booking dates
        var bookings = room.Bookings.Where(b => b.CheckInDate <= booking.CheckOutDate && b.CheckOutDate >= booking.CheckInDate).ToList();

        if (bookings.Count != 0)
        {
            services.Logger.LogInformation("Room is not available for the booking dates");
            return TypedResults.BadRequest();
        }

        if (booking.Id == Guid.Empty)
            booking.Id = Guid.CreateVersion7();

        booking.RoomId = id;
        booking.BookingDate = DateTime.UtcNow;
        booking.Status = BookingStatus.Pending;
        bookings.Add(booking);

        await services.DbContext.SaveChangesAsync();

        services.Logger.LogInformation("Booking {id} created successfully", booking.Id);

        return TypedResults.Ok(booking);
    }

    internal static async Task<Results<Ok<Booking>, BadRequest, NotFound>> ConfirmBooking([AsParameters] ApiServices services, Guid bookingId)
    {
        var booking = await services.DbContext.Bookings.FindAsync(bookingId);
        if (booking == null)
        {
            return TypedResults.NotFound();
        }

        if (booking.Status != BookingStatus.Pending)
        {
            services.Logger.LogInformation("Booking {id} is not in pending state", booking.Id);
            return TypedResults.BadRequest();
        }

        booking.Status = BookingStatus.Confirmed;
        
        await services.DbContext.SaveChangesAsync();
        services.Logger.LogInformation("Booking {id} confirmed successfully", booking.Id);

        return TypedResults.Ok(booking);
    }

}
