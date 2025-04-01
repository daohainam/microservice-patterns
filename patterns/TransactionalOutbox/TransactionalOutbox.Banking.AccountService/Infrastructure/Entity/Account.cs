namespace TransactionalOutbox.Banking.AccountService.Infrastructure.Entity;
public class Account
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal CurrentCredit { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime BalanceChangedAtUtc { get; set; }
    public List<Transaction> Transactions { get; set; } = [];
}
