using EventSourcing.Banking.AccountService.Validators;

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
    public static async Task<Results<Ok, BadRequest<string>>> OpenAccount([AsParameters] ApiServices services, [OpenAccountRequestValidation] OpenAccountRequest request)
    {
        if (request.Id == Guid.Empty)
        {
            request.Id = Guid.CreateVersion7();
        }

        var account = Account.Create(request.Id, request.AccountNumber, request.Currency, request.Balance, request.CreditLimit);

        await services.EventStore.AppendAsync(account, StreamStates.New, cancellationToken: services.CancellationToken);

        services.Logger.LogInformation("Account opened: {AccountId}", account.Id);

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

    internal static async Task<Results<BadRequest<string>, NotFound, Ok>> Deposit([AsParameters] ApiServices services, Guid id, DepositRequest deposit)
    {
        if (deposit is null)
        {
            return TypedResults.BadRequest("Request cannot be null");
        }

        if (id == Guid.Empty)
        {
            return TypedResults.BadRequest("Invalid account ID");
        }

        if (deposit.Amount <= 0)
        {
            return TypedResults.BadRequest("Deposit amount must be positive");
        }

        var account = await services.EventStore.FindAsync<Account>(id,
            typeResolver: TypeResolver,
            cancellationToken: services.CancellationToken);

        if (account is null)
        {
            return TypedResults.NotFound();
        }

        account.Deposit(deposit.Amount);

        await services.EventStore.AppendAsync(account, cancellationToken: services.CancellationToken);

        services.Logger.LogInformation("Account deposited: {AccountId}, amount: {Amount}", account.Id, deposit.Amount);

        return TypedResults.Ok();
    }

    internal static async Task<Results<BadRequest<string>, NotFound, Ok>> Withdraw([AsParameters] ApiServices services, Guid id, WithdrawRequest withdraw)
    {
        if (withdraw is null)
        {
            return TypedResults.BadRequest("Request cannot be null");
        }

        if (id == Guid.Empty)
        {
            return TypedResults.BadRequest("Invalid account ID");
        }

        if (withdraw.Amount <= 0)
        {
            return TypedResults.BadRequest("Withdraw amount must be positive");
        }

        var account = await services.EventStore.FindAsync<Account>(id,
            typeResolver: TypeResolver,
            cancellationToken: services.CancellationToken);

        if (account is null)
        {
            return TypedResults.NotFound();
        }

        try
        {
            account.Withdraw(withdraw.Amount);
        }
        catch (InvalidOperationException ex)
        {
            services.Logger.LogWarning(ex, "Account withdraw failed: {AccountId}, amount: {Amount}", account.Id, withdraw.Amount);
         
            return TypedResults.BadRequest(ex.Message);
        }

        await services.EventStore.AppendAsync(account, cancellationToken: services.CancellationToken);

        services.Logger.LogInformation("Account withdrawed: {AccountId}, amount: {Amount}", account.Id, withdraw.Amount);

        return TypedResults.Ok();
    }

    private static Type TypeResolver(string typeName) => Type.GetType(typeName) ?? throw new InvalidOperationException($"Type '{typeName}' not found.");

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
