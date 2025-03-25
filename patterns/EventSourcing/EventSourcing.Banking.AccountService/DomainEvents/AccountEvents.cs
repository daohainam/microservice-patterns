namespace EventSourcing.Banking.AccountService.DomainEvents;

public abstract record AccountEvent(Guid AccountId, DateTime TimeStamp) : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
}
public record AccountOpenedEvent(Guid AccountId, string AccountNumber, string Currency, decimal InitialBalance, decimal CreditLimit) : AccountEvent(AccountId, DateTime.UtcNow)
{
}
public record MoneyDepositedEvent(Guid AccountId, decimal Amount) : AccountEvent(AccountId, DateTime.UtcNow)
{
}
public record MoneyWithdrawnEvent(Guid AccountId, decimal Amount) : AccountEvent(AccountId, DateTime.UtcNow)
{
}
public record AccountClosedEvent(Guid AccountId) : AccountEvent(AccountId, DateTime.UtcNow)
{
}
public record CreditLimitAssignedEvent(Guid AccountId, decimal CreditLimit) : AccountEvent(AccountId, DateTime.UtcNow)
{
}
