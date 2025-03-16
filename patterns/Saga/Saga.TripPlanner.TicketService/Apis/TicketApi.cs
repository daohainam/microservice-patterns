namespace Saga.TripPlanner.TicketService.Apis;
public static class TicketApiExtensions
{
    public static IEndpointRouteBuilder MapTicketApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/saga/v1")
              .MapTicketApi()
              .WithTags("Ticket Api");

        return builder;
    }

    public static RouteGroupBuilder MapTicketApi(this RouteGroupBuilder group)
    {
        group.MapGet("ticket-types", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.TicketTypes.ToListAsync();
        });

        group.MapPost("tickets", TicketApi.CreateTicket);
        group.MapPut("tickets/{id:guid}/confirm", TicketApi.ConfirmTicket);


        return group;
    }
}
public class TicketApi
{ 
    internal static async Task<Results<Ok<Ticket>, BadRequest>> CreateTicket([AsParameters] ApiServices services, Ticket ticket)
    {
        if (ticket == null) {
            return TypedResults.BadRequest();
        }

        if (ticket.Id == Guid.Empty)
            ticket.Id = Guid.CreateVersion7();

        await services.DbContext.Tickets.AddAsync(ticket);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(ticket);
    }

    internal static async Task<Results<Ok<Ticket>, BadRequest, NotFound>> ConfirmTicket([AsParameters] ApiServices services, Guid id)
    {
        var ticket = await services.DbContext.Tickets.FindAsync(id);
        if (ticket == null)
        {
            return TypedResults.NotFound();
        }

        if (ticket.Status != TicketStatus.Pending)
        {
            services.Logger.LogInformation("Ticket {id} is not in pending state", ticket.Id);
            return TypedResults.BadRequest();
        }

        ticket.Status = TicketStatus.Confirmed;

        await services.DbContext.SaveChangesAsync();
        services.Logger.LogInformation("Ticket {id} confirmed successfully", ticket.Id);

        return TypedResults.Ok(ticket);
    }

}
