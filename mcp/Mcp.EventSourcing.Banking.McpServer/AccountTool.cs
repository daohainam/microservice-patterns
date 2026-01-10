using ModelContextProtocol.Server;
using System.Text.Json;

namespace Mcp.EventSourcing.Banking.McpServer;

[McpServerToolType]
public class AccountTool
{
    [McpServerTool]
    public static async Task<string> OpenAccount(IAccountService accountService, string accountNumber, string currency, decimal initialBalance, decimal creditLimit, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new ArgumentException("Account number is required.", nameof(accountNumber));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        if (initialBalance < 0)
        {
            throw new ArgumentException("Initial balance cannot be negative.", nameof(initialBalance));
        }

        if (creditLimit < 0)
        {
            throw new ArgumentException("Credit limit cannot be negative.", nameof(creditLimit));
        }

        var account = await accountService.OpenAccount(accountNumber, currency, initialBalance, creditLimit, cancellationToken);
        return JsonSerializer.Serialize(account);
    }

    [McpServerTool]
    public static async Task<string> GetAccount(IAccountService accountService, string accountId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(accountId, out var parsedAccountId))
        {
            throw new ArgumentException($"Invalid account ID format: '{accountId}'. Expected a valid GUID.", nameof(accountId));
        }

        var account = await accountService.GetAccountById(parsedAccountId, cancellationToken);
        return JsonSerializer.Serialize(account);
    }

    [McpServerTool]
    public static async Task<string> Deposit(IAccountService accountService, string accountId, decimal amount, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(accountId, out var parsedAccountId))
        {
            throw new ArgumentException($"Invalid account ID format: '{accountId}'. Expected a valid GUID.", nameof(accountId));
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be positive.", nameof(amount));
        }

        await accountService.Deposit(parsedAccountId, amount, cancellationToken);
        return JsonSerializer.Serialize(new { message = "Deposit successful", accountId = parsedAccountId, amount });
    }

    [McpServerTool]
    public static async Task<string> Withdraw(IAccountService accountService, string accountId, decimal amount, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(accountId, out var parsedAccountId))
        {
            throw new ArgumentException($"Invalid account ID format: '{accountId}'. Expected a valid GUID.", nameof(accountId));
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Withdrawal amount must be positive.", nameof(amount));
        }

        await accountService.Withdraw(parsedAccountId, amount, cancellationToken);
        return JsonSerializer.Serialize(new { message = "Withdrawal successful", accountId = parsedAccountId, amount });
    }
}
