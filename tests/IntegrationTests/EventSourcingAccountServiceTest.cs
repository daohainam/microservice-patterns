using Aspire.Hosting;
using EventSourcing.Banking.AccountService.Apis;
using EventSourcing.Banking.AccountService.Infrastructure.Entity;
using System.Net.Http.Json;

namespace IntegrationTests.Tests;

public class EventSourcingAccountServiceTest
{
    private readonly AppFixture fixture;
    private DistributedApplication App => fixture.App;

    public EventSourcingAccountServiceTest(AppFixture fixture)
    {
        this.fixture = fixture;
    }


    [Theory]
    [InlineData("1234567890", "USD", 100, 2)]
    [InlineData("1234567891", "USD", 9999999999, 0)]
    [InlineData("1234567892", "USD", 100, 2)]
    [InlineData("1234567893", "USD", 100, 2)]
    public async Task Create_Account_And_Reread_Success(string accountNumber, string currency, decimal balance, decimal creditLimit)
    {
        // Arrange
        var accountHttpClient = App.CreateHttpClient<Projects.EventSourcing_Banking_AccountService>();
        
        var request = new OpenAccountRequest()
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            Currency = currency,
            Balance = balance,
            CreditLimit = creditLimit
        };

        // Act
        var response = await accountHttpClient.PostAsJsonAsync("/api/eventsourcing/v1/accounts", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        response = await accountHttpClient.GetAsync($"/api/eventsourcing/v1/accounts/{request.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        var account = await response.Content.ReadFromJsonAsync<Account>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(account);
        Assert.Equal(request.Id, account.Id);
        Assert.Equal(request.AccountNumber, account.AccountNumber);
        Assert.Equal(request.Currency, account.Currency);
        Assert.Equal(request.Balance, account.Balance);
        Assert.Equal(request.CreditLimit, account.CreditLimit);
    }

    [Theory]
    [InlineData("DS1234567890", "USD", 100, 0, 50)]
    [InlineData("DS1234567891", "USD", 100, 0, 100)]
    [InlineData("DS1234567892", "USD", 100, 0, 0)]
    [InlineData("DS1234567892", "USD", 100, 100, 200)]
    [InlineData("DS1234567893", "USD", 0, 0, 0)]
    [InlineData("DS1234567894", "USD", 999999, 0, 111111)]
    public async Task Deposit_Test_Success(string accountNumber, string currency, decimal balance, decimal creditLimit, decimal amount)
    {
        // Arrange
        var accountHttpClient = App.CreateHttpClient<Projects.EventSourcing_Banking_AccountService>();

        var request = new OpenAccountRequest()
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            Currency = currency,
            Balance = balance,
            CreditLimit = creditLimit
        };

        // Act
        var response = await accountHttpClient.PostAsJsonAsync("/api/eventsourcing/v1/accounts", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        response = await accountHttpClient.PutAsJsonAsync($"/api/eventsourcing/v1/accounts/{request.Id}/deposit", new DepositRequest() { Amount = amount }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        response = await accountHttpClient.GetAsync($"/api/eventsourcing/v1/accounts/{request.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        var account = await response.Content.ReadFromJsonAsync<Account>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(account);
        Assert.Equal(request.Id, account.Id);
        Assert.Equal(request.AccountNumber, account.AccountNumber);
        Assert.Equal(request.Currency, account.Currency);
        Assert.Equal(balance + amount, account.Balance);
        Assert.Equal(request.CreditLimit, account.CreditLimit);
    }

    [Theory]
    [InlineData("DF1234567890", "USD", 100, 0, 120)]
    [InlineData("DF1234567891", "USD", 100, 100, 201)]
    [InlineData("DF1234567892", "USD", 0, 0, 1)]
    public async Task Deposit_Test_Failed(string accountNumber, string currency, decimal balance, decimal creditLimit, decimal amount)
    {
        var accountHttpClient = App.CreateHttpClient<Projects.EventSourcing_Banking_AccountService>();

        var request = new OpenAccountRequest()
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            Currency = currency,
            Balance = balance,
            CreditLimit = creditLimit
        };

        // Act
        var response = await accountHttpClient.PostAsJsonAsync("/api/eventsourcing/v1/accounts", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        response = await accountHttpClient.PutAsJsonAsync($"/api/eventsourcing/v1/accounts/{request.Id}/withdraw", new DepositRequest() { Amount = amount }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("DWS1234567890", "USD", 100, 0, 50, 150, 0, 0)]
    [InlineData("DWS1234567891", "USD", 100, 200, 300, 199, 201, 0)]
    [InlineData("DWS1234567892", "USD", 100, 200, 300, 599, 0, 199)]
    public async Task Deposit_And_Withdraw_Test_Success(string accountNumber, string currency, decimal balance, decimal creditLimit, decimal deposit, decimal withdraw, decimal lastBalance, decimal lastCredit)
    {
        // Arrange
        var accountHttpClient = App.CreateHttpClient<Projects.EventSourcing_Banking_AccountService>();

        var request = new OpenAccountRequest()
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            Currency = currency,
            Balance = balance,
            CreditLimit = creditLimit
        };

        // Act
        var response = await accountHttpClient.PostAsJsonAsync("/api/eventsourcing/v1/accounts", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        response = await accountHttpClient.PutAsJsonAsync($"/api/eventsourcing/v1/accounts/{request.Id}/deposit", new DepositRequest() { Amount = deposit }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        response = await accountHttpClient.PutAsJsonAsync($"/api/eventsourcing/v1/accounts/{request.Id}/withdraw", new DepositRequest() { Amount = withdraw }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        response = await accountHttpClient.GetAsync($"/api/eventsourcing/v1/accounts/{request.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        var account = await response.Content.ReadFromJsonAsync<Account>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(account);
        Assert.Equal(request.Id, account.Id);
        Assert.Equal(request.AccountNumber, account.AccountNumber);
        Assert.Equal(request.Currency, account.Currency);
        Assert.Equal(lastBalance, account.Balance);
        Assert.Equal(request.CreditLimit, account.CreditLimit);
        Assert.Equal(lastCredit, account.CurrentCredit);
    }

}
