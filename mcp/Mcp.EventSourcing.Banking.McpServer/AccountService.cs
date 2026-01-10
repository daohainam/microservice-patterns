using System.Text.Json;

namespace Mcp.EventSourcing.Banking.McpServer;

public class AccountService(HttpClient accountHttpClient) : IAccountService
{
    public async Task<Account> OpenAccount(string accountNumber, string currency, decimal initialBalance, decimal creditLimit, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            Id = Guid.Empty,
            AccountNumber = accountNumber,
            Currency = currency,
            Balance = initialBalance,
            CreditLimit = creditLimit
        };

        var response = await accountHttpClient.PostAsJsonAsync("/api/eventsourcing/v1/accounts", request, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        
        // The API returns Ok with no body, so we need to construct the response
        return new Account
        {
            Id = Guid.NewGuid(), // Note: In a real scenario, the API should return the created ID
            AccountNumber = accountNumber,
            Currency = currency,
            Balance = initialBalance,
            CreditLimit = creditLimit,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public async Task<Account> GetAccountById(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await accountHttpClient.GetFromJsonAsync<Account>($"/api/eventsourcing/v1/accounts/{accountId}", cancellationToken: cancellationToken) ?? throw new InvalidOperationException($"Failed to retrieve account {accountId}");
        return account;
    }

    public async Task Deposit(Guid accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        var request = new { Amount = amount };
        var response = await accountHttpClient.PutAsJsonAsync($"/api/eventsourcing/v1/accounts/{accountId}/deposit", request, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task Withdraw(Guid accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        var request = new { Amount = amount };
        var response = await accountHttpClient.PutAsJsonAsync($"/api/eventsourcing/v1/accounts/{accountId}/withdraw", request, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
