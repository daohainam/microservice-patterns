namespace EventSourcing.Banking.AccountService.DomainEvents;

public abstract class AccountEvent : IDomainEvent
{
    public Guid AccountId { get; }
    public Guid EventId { get; set; }
    public long Version { get; set; }
    public DateTime TimestampUtc { get; set; }

    protected AccountEvent(Guid accountId, DateTime timestampUtc)
    {
        AccountId = accountId;
        EventId = Guid.CreateVersion7();
        TimestampUtc = timestampUtc;
    }
}

public class AccountOpenedEvent : AccountEvent
{
    public string AccountNumber { get; }
    public string Currency { get; }
    public decimal InitialBalance { get; }
    public decimal CreditLimit { get; }

    public AccountOpenedEvent(Guid accountId, string accountNumber, string currency, decimal initialBalance, decimal creditLimit, DateTime timestampUtc)
        : base(accountId, timestampUtc)
    {
        AccountNumber = accountNumber;
        Currency = currency;
        InitialBalance = initialBalance;
        CreditLimit = creditLimit;
    }
}

public class MoneyDepositedEvent : AccountEvent
{
    public decimal Amount { get; }

    public MoneyDepositedEvent(Guid accountId, decimal amount, DateTime timestampUtc)
        : base(accountId, timestampUtc)
    {
        Amount = amount;
    }
}

public class MoneyWithdrawnEvent : AccountEvent
{
    public decimal Amount { get; }

    public MoneyWithdrawnEvent(Guid accountId, decimal amount, DateTime timestampUtc)
        : base(accountId, timestampUtc)
    {
        Amount = amount;
    }
}

public class AccountClosedEvent : AccountEvent
{
    public AccountClosedEvent(Guid accountId)
        : base(accountId, DateTime.UtcNow)
    {
    }
}

public class CreditLimitAssignedEvent : AccountEvent
{
    public decimal CreditLimit { get; }

    public CreditLimitAssignedEvent(Guid accountId, decimal creditLimit, DateTime timestampUtc)
        : base(accountId, timestampUtc)
    {
        CreditLimit = creditLimit;
    }
}
