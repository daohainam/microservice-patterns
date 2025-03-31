namespace EventSourcing.Banking.AccountService.Infrastructure.Entity;
public class Account: Aggregate
{
    public string AccountNumber { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal CurrentCredit { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime BalanceChangedAtUtc { get; set; }
    public List<Transaction> Transactions { get; set; } = [];


    public Account() { }

    public static Account Create(Guid id, string accountNumber, string currency, decimal initialBalance, decimal creditLimit)
    {
        var account = new Account
        {
            Id = id,
            AccountNumber = accountNumber,
            Currency = currency,
            Balance = initialBalance,
            CurrentCredit = 0,
            CreditLimit = creditLimit,
            CreatedAtUtc = DateTime.UtcNow,
            BalanceChangedAtUtc = DateTime.UtcNow,
            IsClosed = false,
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

    #region Apply methods
    public void Apply(AccountOpenedEvent evt)
    {
        Id = evt.AccountId;
        AccountNumber = evt.AccountNumber;
        Currency = evt.Currency;
        Balance = evt.InitialBalance;
        CreatedAtUtc = evt.TimeStamp;
        BalanceChangedAtUtc = evt.TimeStamp;
        CreditLimit = evt.CreditLimit;
        IsClosed = false;
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

        if (CurrentCredit > 0)
        {
            var creditToPay = Math.Min(CurrentCredit, evt.Amount);
            
            CurrentCredit -= creditToPay;
            Balance += evt.Amount - creditToPay;
        }
        else
        {
            Balance += evt.Amount;
        }

        BalanceChangedAtUtc = evt.TimeStamp;
    }

    public void Apply(MoneyWithdrawnEvent evt)
    {
        if (evt.AccountId != Id)
        {
            throw new InvalidOperationException("Invalid account id");
        }

        if (IsClosed)
        {
            throw new InvalidOperationException("Account is closed");
        }

        if (Balance + (CreditLimit - CurrentCredit) < evt.Amount)
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

        if (Balance >= evt.Amount)
        {
            Balance -= evt.Amount;
        }
        else
        {
            CurrentCredit += evt.Amount - Balance;

            if (CurrentCredit > CreditLimit)
            {
                throw new InvalidOperationException("Credit limit exceeded");
            }

            Balance = 0;
        }

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
    #endregion
}
