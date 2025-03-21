using Aspire.Hosting;
using Saga.TripPlanner.HotelService.Infrastructure.Entity;
using Saga.TripPlanner.PaymentService.Infrastructure.Entity;
using Saga.TripPlanner.TicketService.Infrastructure.Entity;
using System.Net.Http.Json;

namespace IntegrationTests.Tests;

public class SagaTripPlannerServiceTest
{
    private readonly AppFixture fixture;
    private DistributedApplication App => fixture.App;

    public SagaTripPlannerServiceTest(AppFixture fixture)
    {
        this.fixture = fixture;
    }


    [Theory]
    [InlineData(100, 2)]
    [InlineData(1000, 20)]
    [InlineData(10000, 200)]
    [InlineData(100000, 2000)]
    [InlineData(9999, 9999)]
    public async Task Create_Hotel_Room_Success(decimal price, int maxOccupancy)
    {
        var hotelHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_HotelService>();
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = "Test Room",
            Price = price,
            MaxOccupancy = maxOccupancy
        };
        var response = await hotelHttpClient.PostAsJsonAsync("/api/saga/v1/rooms", room, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        var result = await response.Content.ReadFromJsonAsync<Room>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.Id);
        Assert.Equal(room.Name, result.Name);
        Assert.Equal(room.Price, result.Price);
        Assert.Equal(room.MaxOccupancy, result.MaxOccupancy);

        // Act
        response = await hotelHttpClient.GetAsync($"/api/saga/v1/rooms/{room.Id}", TestContext.Current.CancellationToken);
        result = await response.Content.ReadFromJsonAsync<Room>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.Id);
        Assert.Equal(room.Name, result.Name);
        Assert.Equal(room.Price, result.Price);
        Assert.Equal(room.MaxOccupancy, result.MaxOccupancy);


        // Arrange

    }

    [Theory]
    [InlineData(-100, -2)]
    [InlineData(-1000, 20)]
    [InlineData(10000, -200)]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    public async Task Create_Hotel_Room_Failed(decimal price, int maxOccupancy)
    {
        var hotelHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_HotelService>();
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = "Test Room",
            Price = price,
            MaxOccupancy = maxOccupancy
        };
        var response = await hotelHttpClient.PostAsJsonAsync("/api/saga/v1/rooms", room, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(120)]
    [InlineData(200)]
    public async Task Create_Ticket_Success(decimal price)
    {
        // Arrange
        string ticketTypeId = Guid.NewGuid().ToString();
        var ticketHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_TicketService>();

        var response = await ticketHttpClient.PostAsJsonAsync("/api/saga/v1/ticket-types", new TicketType()
        {
            Id = ticketTypeId,
            Name = $"Test Ticket: {ticketTypeId}",
            Price = price,
            AvailableTickets = 100
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        var ticketTypeResponse = await ticketHttpClient.GetAsync($"/api/saga/v1/ticket-types/{ticketTypeId}", TestContext.Current.CancellationToken);
        var ticketType = await ticketTypeResponse.Content.ReadFromJsonAsync<TicketType>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, ticketTypeResponse.StatusCode);
        Assert.NotNull(ticketType);
        Assert.Equal(ticketTypeId, ticketType.Id);

        // Act
        var ticket = new Ticket()
        {
            Id = Guid.NewGuid(),
            TicketTypeId = ticketTypeId
        };
        response = await ticketHttpClient.PostAsJsonAsync("/api/saga/v1/tickets", ticket, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        var result = await response.Content.ReadFromJsonAsync<Ticket>(cancellationToken: TestContext.Current.CancellationToken);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(price, result.Price);
        Assert.Equal(ticket.TicketTypeId, result.TicketTypeId);

        // Act
        response = await ticketHttpClient.GetAsync($"/api/saga/v1/tickets/{ticket.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        result = await response.Content.ReadFromJsonAsync<Ticket>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(price, result.Price);
        Assert.Equal(ticket.TicketTypeId, result.TicketTypeId);
    }

    [Fact]
    public async Task Create_Ticket_TicketType_NotFound()
    {
        var ticketHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_TicketService>();

        var ticket = new Ticket()
        {
            Id = Guid.NewGuid(),
            TicketTypeId = "ticket_type_not_found"
        };
        var response = await ticketHttpClient.PostAsJsonAsync("/api/saga/v1/tickets", ticket, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(500)]
    [InlineData(999999)]
    public async Task Create_Credit_Card_Success(decimal creditLimit)
    {
        var paymentHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_PaymentService>();
        var creditCard = new CreditCard()
        {
            Id = Guid.NewGuid(),
            CardNumber = GenerateCreditCardNumber(),
            CardHolderName = "Test User",
            ExpirationDate = "12/23",
            Cvv = "123",
            CreditLimit = creditLimit,
            AvailableCredit = creditLimit
        };
        var response = await paymentHttpClient.PostAsJsonAsync("/api/saga/v1/cards", creditCard, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        var result = await response.Content.ReadFromJsonAsync<CreditCard>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(creditCard.Id, result.Id);
        Assert.Equal(creditCard.CardNumber, result.CardNumber);
        Assert.Equal(creditCard.CardHolderName, result.CardHolderName);
        Assert.Equal(creditCard.ExpirationDate, result.ExpirationDate);
        Assert.Equal(creditCard.Cvv, result.Cvv);
        Assert.Equal(creditCard.CreditLimit, result.CreditLimit);
        Assert.Equal(creditCard.AvailableCredit, result.AvailableCredit);

        // Act
        response = await paymentHttpClient.GetAsync($"/api/saga/v1/cards/{creditCard.Id}", TestContext.Current.CancellationToken);
        result = await response.Content.ReadFromJsonAsync<CreditCard>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(creditCard.Id, result.Id);
        Assert.Equal(creditCard.CardNumber, result.CardNumber);
        Assert.Equal(creditCard.CardHolderName, result.CardHolderName);
        Assert.Equal(creditCard.ExpirationDate, result.ExpirationDate);
        Assert.Equal(creditCard.Cvv, result.Cvv);
        Assert.Equal(creditCard.CreditLimit, result.CreditLimit);
        Assert.Equal(creditCard.AvailableCredit, result.AvailableCredit);

        // Act

    }

    private static string GenerateCreditCardNumber() => Guid.NewGuid().ToString()[..16];
}
