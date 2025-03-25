using EventSourcing.Banking.AccountService.Infrastructure.Entity;
using EventSourcing.Infrastructure.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;

namespace EventSourcing.Banking.AccountService.Apis;
public static class AccountApiExetensions
{
    public static IEndpointRouteBuilder MapAccountApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/eventsourcing/v1")
              .MapAccountApi()
              .WithTags("Event Sourcing Account Api");

        return builder;
    }

    public static RouteGroupBuilder MapAccountApi(this RouteGroupBuilder group)
    {
        group.MapPost("accounts", AccountApi.OpenAccount);

        return group;
    }
}
public static class AccountApi
{
    public static async Task<Results<Ok, BadRequest>> OpenAccount([AsParameters] ApiServices services, OpenAccountRequest request)
    {
        if (request == null) {
            return TypedResults.BadRequest();
        }

        if (request.Id == Guid.Empty)
            request.Id = Guid.CreateVersion7();

        var account = Account.Create(request.Id, request.AccountNumber, request.Currency, request.Balance, request.CreditLimit);

        var events = new List<Event>();
        foreach (var evt in account.PendingChanges)
        {
            events.Add(new Event
            {
                Id = evt.EventId,
                StreamId = account.Id,
                Data = JsonSerializer.Serialize(evt),
                Type = evt.GetType().Name,
                CreatedAtUtc = evt.TimeStamp
            });
        }

        await services.EventStore.AppendAsync(account.Id, StreamStates.New, events);

        return TypedResults.Ok();
    }
}

public class OpenAccountRequest
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal CreditLimit { get; set; }

}
