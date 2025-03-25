namespace EventSourcing.Banking.AccountService.Infrastructure.Entity;
public class Account
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime BalanceChangedAtUtc { get; set; }
    public List<Transaction> Transactions { get; set; } = [];

    public long Version { get; set; }
    public IEnumerable<AccountEvent> PendingChanges => _changes;

    private readonly List<AccountEvent> _changes = [];

    private Account() { }

    public static Account Create(Guid id, string accountNumber, string currency, decimal initialBalance, decimal creditLimit)
    {
        var account = new Account
        {
            Id = id,
            AccountNumber = accountNumber,
            Currency = currency,
            Balance = initialBalance,
            CreditLimit = creditLimit,
            Version = 0
        };

        var evt = new AccountOpenedEvent(id, accountNumber, currency, initialBalance, creditLimit);
        account.Apply(evt);
        account._changes.Add(evt);

        return account;
    }

    public void Deposit(decimal amount)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Account is closed");
        }
     
        var evt = new MoneyDepositedEvent(Id, amount);
        Apply(evt);
        _changes.Add(evt);
    }

    public void Withdraw(decimal amount)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Account is closed");
        }

        if (Balance < amount)
        {
            throw new InvalidOperationException("Insufficient funds");
        }
        
        var evt = new MoneyWithdrawnEvent(Id, amount);
        Apply(evt);
        _changes.Add(evt);
    }

    public void Close()
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Account is already closed");
        }
        var evt = new AccountClosedEvent(Id);
        Apply(evt);
        _changes.Add(evt);
    }

    public void AssignCreditLimit(decimal creditLimit)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Account is closed");
        }
        var evt = new CreditLimitAssignedEvent(Id, creditLimit);
        Apply(evt);
        _changes.Add(evt);
    }

    public void Apply(AccountOpenedEvent evt)
    {
        Id = evt.AccountId;
        AccountNumber = evt.AccountNumber;
        Currency = evt.Currency;
        Balance = evt.InitialBalance;
        CreatedAtUtc = evt.TimeStamp;
        BalanceChangedAtUtc = evt.TimeStamp;
    }

    public void Apply(MoneyDepositedEvent evt)
    {
        if (evt.AccountId != Id)
        {
            throw new InvalidOperationException("Invalid account id");
        }

        Transactions.Add(new Transaction
        {
            Id = evt.EventId,
            AccountId = Id,
            Amount = evt.Amount,
            TimeStamp = evt.TimeStamp
        });

        Balance += evt.Amount;
        BalanceChangedAtUtc = evt.TimeStamp;
    }

    public void Apply(MoneyWithdrawnEvent evt)
    {
        if (evt.AccountId != Id)
        {
            throw new InvalidOperationException("Invalid account id");
        }

        if (Balance - evt.Amount > -CreditLimit)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        Transactions.Add(new Transaction
        {
            Id = evt.EventId,
            AccountId = Id,
            Amount = -evt.Amount,
            TimeStamp = evt.TimeStamp
        });

        Balance -= evt.Amount;
        BalanceChangedAtUtc = evt.TimeStamp;
    }

    public void Apply(AccountClosedEvent evt)
    {
        if (evt.AccountId != Id)
        {
            throw new InvalidOperationException("Invalid account id");
        }

        if (IsClosed)
        {
            throw new InvalidOperationException("Account is already closed");
        }

        IsClosed = true;
    }

    public void Apply(CreditLimitAssignedEvent evt)
    {
        if (evt.AccountId != Id)
        {
            throw new InvalidOperationException("Invalid account id");
        }

        if (IsClosed) {
            throw new InvalidOperationException("Account is closed");
        }

        CreditLimit = evt.CreditLimit;
    }

}
