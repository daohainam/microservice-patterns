
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

        group.MapGet("ticket-types/{id}", async ([AsParameters] ApiServices services, string id) =>
        {
            return await services.DbContext.TicketTypes.FindAsync(id);
        });

        group.MapPost("ticket-types", TicketApi.CreateTicketType);

        group.MapGet("tickets", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.Tickets.ToListAsync();
        });

        group.MapGet("tickets/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.Tickets.FindAsync(id);
        });

        group.MapPost("tickets", TicketApi.CreateTickets);
        group.MapPut("tickets/{id:guid}/confirm", TicketApi.CancelTickets);


        return group;
    }
}
public class TicketApi
{
    internal static async Task<Results<Ok<TicketType>, BadRequest, Conflict>> CreateTicketType([AsParameters] ApiServices services, TicketType tickeType)
    {
        if (tickeType == null)
        {
            return TypedResults.BadRequest();
        }
        
        if (string.IsNullOrWhiteSpace(tickeType.Id))
        {
            services.Logger.LogInformation("Ticket type id is required");
            return TypedResults.BadRequest();            
        }

        if (string.IsNullOrWhiteSpace(tickeType.Name))
        {
            services.Logger.LogInformation("Ticket type name is required");
            return TypedResults.BadRequest();
        }

        if (tickeType.Price <= 0)
        {
            services.Logger.LogInformation("Ticket type price must be greater than 0");
            return TypedResults.BadRequest();
        }

        if (tickeType.AvailableTickets < 0)
        {
            services.Logger.LogInformation("Ticket type available tickets must be greater than or equal to 0");
            return TypedResults.BadRequest();
        }

        if (await services.DbContext.TicketTypes.AnyAsync(x => x.Id == tickeType.Id))
        {
            services.Logger.LogInformation("Ticket type {id} already exists", tickeType.Id);
            return TypedResults.Conflict();
        }

        await services.DbContext.TicketTypes.AddAsync(tickeType);
        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(tickeType);

    }

    internal static async Task<Results<Ok<List<Ticket>>, BadRequest>> CreateTickets([AsParameters] ApiServices services, List<Ticket> tickets)
    {
        if (tickets == null || tickets.Count == 0) {
            return TypedResults.BadRequest();
        }

        foreach (Ticket ticket in tickets)
        {
            var ticketType = await services.DbContext.TicketTypes.FindAsync(ticket.TicketTypeId);
            if (ticketType == null)
            {
                services.Logger.LogInformation("Ticket type {id} not found", ticket.TicketTypeId);
                return TypedResults.BadRequest();
            }

            if (ticketType.AvailableTickets <= 0)
            {
                services.Logger.LogInformation("Ticket type {id} is sold out", ticket.TicketTypeId);
                return TypedResults.BadRequest();
            }

            if (ticket.Id == Guid.Empty)
                ticket.Id = Guid.CreateVersion7();

            ticket.Price = ticketType.Price;
            ticket.Status = TicketStatus.Booked;

            ticketType.AvailableTickets--;

            await services.DbContext.Tickets.AddAsync(ticket);
        }

        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(tickets);
    }

    internal static async Task<Results<Ok, BadRequest, NotFound>> CancelTickets([AsParameters] ApiServices services, List<Guid> ticketIds)
    {
        if (ticketIds == null || ticketIds.Count == 0)
        {
            return TypedResults.BadRequest();
        }

        foreach (Guid id in ticketIds)
        {
            var ticket = await services.DbContext.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return TypedResults.NotFound();
            }

            if (ticket.Status != TicketStatus.Booked)
            {
                services.Logger.LogInformation("Ticket {id} is not in Booked state", ticket.Id);
                return TypedResults.BadRequest();
            }

            ticket.Status = TicketStatus.Cancelled;
            ticket.TicketType.AvailableTickets++;
        }

        await services.DbContext.SaveChangesAsync();
        services.Logger.LogInformation("Ticket {ids} confirmed successfully", ticketIds);

        return TypedResults.Ok();
    }

}
