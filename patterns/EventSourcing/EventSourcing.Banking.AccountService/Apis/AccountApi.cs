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
        group.MapGet("accounts/{id:guid}", AccountApi.GetAccountById);
        group.MapPut("accounts/{id:guid}/deposit", AccountApi.Deposit);
        group.MapPut("accounts/{id:guid}/withdraw", AccountApi.Withdraw);

        return group;
    }
}
public class AccountApi
{
    public static async Task<Results<Ok, BadRequest>> OpenAccount([AsParameters] ApiServices services, OpenAccountRequest request)
    {
        if (request == null) {
            return TypedResults.BadRequest();
        }

        if (request.Id == Guid.Empty)
            request.Id = Guid.CreateVersion7();

        var account = Account.Create(request.Id, request.AccountNumber, request.Currency, request.Balance, request.CreditLimit);

        await services.EventStore.AppendAsync(account, StreamStates.New, cancellationToken: services.CancellationToken);

        services.Logger.LogInformation("Account opened: {Id}", account.Id);

        return TypedResults.Ok();
    }

    public static async Task<Results<Ok<Account>, BadRequest, NotFound>> GetAccountById([AsParameters] ApiServices services, Guid id)
    {
        if (id == Guid.Empty)
        {
            return TypedResults.BadRequest();
        }

        var account = await services.EventStore.FindAsync<Account>(id,
            typeResolver: TypeResolver,
            cancellationToken: services.CancellationToken);

        if (account == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(account);
    }

    internal static async Task<Results<BadRequest, NotFound, Ok>> Deposit([AsParameters] ApiServices services, Guid id, DepositRequest deposit)
    {
        if (deposit == null || id == Guid.Empty)
        {
            return TypedResults.BadRequest();
        }

        var account = await services.EventStore.FindAsync<Account>(id,
            typeResolver: TypeResolver,
            cancellationToken: services.CancellationToken);

        if (account == null)
            return TypedResults.NotFound();

        account.Deposit(deposit.Amount);

        await services.EventStore.AppendAsync(account, cancellationToken: services.CancellationToken);

        services.Logger.LogInformation("Account deposited: {Id}, amount: {amount}", account.Id, deposit.Amount);

        return TypedResults.Ok();
    }

    internal static async Task<Results<BadRequest, NotFound, Ok>> Withdraw([AsParameters] ApiServices services, Guid id, WithdrawRequest withdraw)
    {
        if (withdraw == null || id == Guid.Empty)
        {
            return TypedResults.BadRequest();
        }

        var account = await services.EventStore.FindAsync<Account>(id,
            typeResolver: TypeResolver,
            cancellationToken: services.CancellationToken);

        if (account == null)
            return TypedResults.NotFound();

        try
        {
            account.Withdraw(withdraw.Amount);
        }
        catch (InvalidOperationException ex)
        {
            services.Logger.LogError(ex, "Account withdraw failed: {Id}, amount: {amount}", account.Id, withdraw.Amount);
         
            return TypedResults.BadRequest();
        }

        await services.EventStore.AppendAsync(account, cancellationToken: services.CancellationToken);

        services.Logger.LogInformation("Account withdrawed: {Id}, amount: {amount}", account.Id, withdraw.Amount);

        return TypedResults.Ok();
    }

    private static Type TypeResolver(string typeName)
    {
        var type = Type.GetType(typeName);
        return type == null ? throw new InvalidOperationException($"Type '{typeName}' not found.") : type;
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

public record DepositRequest
{
    public decimal Amount { get; set; }
}

public record WithdrawRequest
{
    public decimal Amount { get; set; }
}
