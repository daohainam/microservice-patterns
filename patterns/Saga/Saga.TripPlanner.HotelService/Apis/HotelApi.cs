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

        return group;
    }
}
public class HotelApi
{ 
    public static async Task<Results<Ok<Room>, BadRequest>> CreateRoom([AsParameters] ApiServices services, Room room)
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

}
