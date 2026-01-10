namespace Mcp.EventSourcing.Banking.McpServer;

public interface IAccountService
{
    Task<Account> OpenAccount(string accountNumber, string currency, decimal initialBalance, decimal creditLimit, CancellationToken cancellationToken = default);
    Task<Account> GetAccountById(Guid accountId, CancellationToken cancellationToken = default);
    Task Deposit(Guid accountId, decimal amount, CancellationToken cancellationToken = default);
    Task Withdraw(Guid accountId, decimal amount, CancellationToken cancellationToken = default);
}
