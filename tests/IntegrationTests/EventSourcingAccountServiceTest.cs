using Aspire.Hosting;
using EventSourcing.Banking.AccountService.Apis;
using EventSourcing.Banking.AccountService.Infrastructure.Entity;
using Saga.TripPlanner.HotelService.Infrastructure.Entity;
using Saga.TripPlanner.PaymentService.Infrastructure.Entity;
using Saga.TripPlanner.TicketService.Infrastructure.Entity;
using System.Net.Http;
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
}
