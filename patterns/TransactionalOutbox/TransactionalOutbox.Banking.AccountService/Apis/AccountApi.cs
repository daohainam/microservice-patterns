namespace TransactionalOutbox.Banking.AccountService.Apis;
public static class AccountApiExetensions
{
    public static IEndpointRouteBuilder MapAccountApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/outbox/v1")
              .MapAccountApi()
              .WithTags("Outbox Account Api");

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

        var account = new Account() 
        {
            Id = request.Id,
            AccountNumber = request.AccountNumber,
            Currency = request.Currency,
            Balance = request.Balance,
            CreditLimit = request.CreditLimit,
            BalanceChangedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            CurrentCredit = 0,
            IsClosed = false,
            Transactions = [],            
        };

        await services.UnitOfWork.BeginTransactionAsync(services.CancellationToken);
        
        services.UnitOfWork.AccountDbContext.Accounts.Add(account);

        var evt = new AccountOpenedIntegrationEvent()
        {
            AccountNumber = account.AccountNumber,
            Currency = account.Currency,
            Balance = account.Balance,
            CreditLimit = account.CreditLimit,
        };

        var message = new Abstractions.PollingOutboxMessage()
        {
            CreationDate = DateTime.UtcNow,
            PayloadType = typeof(AccountOpenedIntegrationEvent).FullName ?? throw new Exception($"Could not get fullname of type {evt.GetType()}"),
            Payload = JsonSerializer.Serialize(evt),
            ProcessedDate = null,
        };
        await services.UnitOfWork.OutboxForPollingRepository.AddAsync(message);
        await services.UnitOfWork.OutboxForLogTailingRepository.AddAsync(new Abstractions.LogTailingOutboxMessage() { 
            Id = message.Id,
            CreationDate = message.CreationDate,
            Payload = message.Payload,
            PayloadType = message.PayloadType
        });

        await services.UnitOfWork.AccountDbContext.SaveChangesAsync();
        await services.UnitOfWork.OutboxForPollingRepository.SaveChangesAsync();
        await services.UnitOfWork.OutboxForLogTailingRepository.SaveChangesAsync();

        await services.UnitOfWork.CommitTransactionAsync(services.CancellationToken);

        services.Logger.LogInformation("Account opened: {Id}", account.Id);

        return TypedResults.Ok();
    }

    public static async Task<Results<Ok<Account>, BadRequest, NotFound>> GetAccountById([AsParameters] ApiServices services, Guid id)
    {
        if (id == Guid.Empty)
        {
            return TypedResults.BadRequest();
        }

        var account = await services.UnitOfWork.AccountDbContext.Accounts.FindAsync(id);

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

        await services.UnitOfWork.BeginTransactionAsync(services.CancellationToken);

        var account = await services.UnitOfWork.AccountDbContext.Accounts.FindAsync(id);
        if (account == null)
            return TypedResults.NotFound();

        if (account.CurrentCredit > 0)
        {
            var creditToPay = Math.Min(account.CurrentCredit, deposit.Amount);

            account.CurrentCredit -= creditToPay;
            account.Balance += deposit.Amount - creditToPay;
        }
        else
        {
            account.Balance += deposit.Amount;
        }

        account.BalanceChangedAtUtc = DateTime.UtcNow;

        services.UnitOfWork.AccountDbContext.Transactions.Add(new Transaction
        {
            Id = Guid.CreateVersion7(),
            AccountId = account.Id,
            Amount = deposit.Amount,
            TimeStamp = account.BalanceChangedAtUtc
        });

        var evt = new BalanceChangedIntegrationEvent()
        {
            AccountNumber = account.AccountNumber,            
            Balance = account.Balance,
            Credit = account.CurrentCredit,
        };

        await services.UnitOfWork.OutboxForPollingRepository.AddAsync(new Abstractions.PollingOutboxMessage()
        {
            CreationDate = DateTime.UtcNow,
            PayloadType = typeof(BalanceChangedIntegrationEvent).FullName ?? throw new Exception($"Could not get fullname of type {evt.GetType()}"),
            Payload = JsonSerializer.Serialize(evt),
            ProcessedDate = null,
        });

        await services.UnitOfWork.AccountDbContext.SaveChangesAsync(services.CancellationToken);

        services.Logger.LogInformation("Account deposited: {Id}, amount: {amount}", account.Id, deposit.Amount);

        return TypedResults.Ok();
    }

    internal static async Task<Results<BadRequest, NotFound, Ok>> Withdraw([AsParameters] ApiServices services, Guid id, WithdrawRequest withdraw)
    {
        if (withdraw == null || id == Guid.Empty)
        {
            return TypedResults.BadRequest();
        }

        await services.UnitOfWork.BeginTransactionAsync(services.CancellationToken);

        var account = await services.UnitOfWork.AccountDbContext.Accounts.FindAsync(id);
        if (account == null)
            return TypedResults.NotFound();

        if (account.Balance + (account.CreditLimit - account.CurrentCredit) < withdraw.Amount)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        if (account.Balance >= withdraw.Amount)
        {
            account.Balance -= withdraw.Amount;
        }
        else
        {
            account.CurrentCredit += withdraw.Amount - account.Balance;

            if (account.CurrentCredit > account.CreditLimit)
            {
                throw new InvalidOperationException("Credit limit exceeded");
            }

            account.Balance = 0;
        }

        account.BalanceChangedAtUtc = DateTime.UtcNow;

        services.UnitOfWork.AccountDbContext.Transactions.Add(new Transaction
        {
            Id = Guid.CreateVersion7(),
            AccountId = account.Id,
            Amount = withdraw.Amount,
            TimeStamp = account.BalanceChangedAtUtc
        });

        var evt = new BalanceChangedIntegrationEvent()
        {
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            Credit = account.CurrentCredit,
        };

        await services.UnitOfWork.OutboxForPollingRepository.AddAsync(new Abstractions.PollingOutboxMessage()
        {
            CreationDate = DateTime.UtcNow,
            PayloadType = typeof(BalanceChangedIntegrationEvent).FullName ?? throw new Exception($"Could not get fullname of type {evt.GetType()}"),
            Payload = JsonSerializer.Serialize(evt),
            ProcessedDate = null,
        });

        await services.UnitOfWork.AccountDbContext.SaveChangesAsync(services.CancellationToken);

        services.Logger.LogInformation("Account withdrawed: {Id}, amount: {amount}", account.Id, withdraw.Amount);

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

public record DepositRequest
{
    public decimal Amount { get; set; }
}

public record WithdrawRequest
{
    public decimal Amount { get; set; }
}
