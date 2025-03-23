using Aspire.Hosting;
using Saga.TripPlanner.HotelService.Infrastructure.Entity;
using Saga.TripPlanner.PaymentService.Infrastructure.Entity;
using Saga.TripPlanner.TicketService.Infrastructure.Entity;
using Saga.TripPlanner.TripPlanningService.Infrastructure.Entity;
using System.Net.Http.Json;

namespace IntegrationTests.Tests;

public class SagaTripPlannerIntegrationTest 
{
    private readonly AppFixture fixture;
    private DistributedApplication App => fixture.App;

    public SagaTripPlannerIntegrationTest(AppFixture fixture)
    {
        this.fixture = fixture;
    }


    [Theory]
    [InlineData(100, 2, 200)]
    [InlineData(1000, 20, 200)]
    [InlineData(10000, 20, 10000 + 100)]
    [InlineData(100000, 20, 100000 + 200)]
    [InlineData(9999, 1, 9999 + 50)]
    public async Task Create_TripPlanning_State_Change_To_Confirmed_Success(decimal price, int maxOccupancy, decimal creditLimit)
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
        string ticketTypeId = Guid.NewGuid().ToString();
        var ticketType = new TicketType()
        {
            Id = ticketTypeId,
            Name = $"Test Ticket: {ticketTypeId}",
            Price = price,
            AvailableTickets = 1
        };
        var ticketHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_TicketService>();

        response = await ticketHttpClient.PostAsJsonAsync("/api/saga/v1/ticket-types", ticketType, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Conflict);

        // Act
        var paymentHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_PaymentService>();
        var creditCard = new CreditCard()
        {
            Id = Guid.NewGuid(),
            CardNumber = Guid.CreateVersion7().ToString("N")[..16],
            CardHolderName = "Test User",
            ExpirationDate = "12/23",
            Cvv = "123",
            CreditLimit = creditLimit,
            AvailableCredit = creditLimit
        };
        response = await paymentHttpClient.PostAsJsonAsync("/api/saga/v1/cards", creditCard, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        var tripPlanningHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_TripPlanningService>();
        var trip = new Trip()
        {
            Id = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Name = "Test Trip",
            TicketBookings =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    BookingDate = DateTime.UtcNow,
                    TicketTypeId = ticketTypeId
                }
            ],
            HotelRoomBookings =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    BookingDate = DateTime.UtcNow,
                    CheckInDate = DateTime.UtcNow,
                    CheckOutDate = DateTime.UtcNow.AddDays(1),
                    RoomId = room.Id
                }
            ],
            CardNumber = creditCard.CardNumber,
            CardHolderName = creditCard.CardHolderName,
            ExpirationDate = creditCard.ExpirationDate,
            Cvv = creditCard.Cvv,
        };

        trip.Amount = room.Price * (decimal)Math.Floor((trip.EndDate - trip.StartDate).TotalDays) + trip.TicketBookings.Count * ticketType.Price;

        response = await tripPlanningHttpClient.PostAsJsonAsync("/api/saga/v1/trips", trip, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        var tripResponse = await response.Content.ReadFromJsonAsync<Trip>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(tripResponse);
        Assert.Equal(trip.Name, tripResponse.Name);
        Assert.Equal(trip.StartDate, tripResponse.StartDate);
        Assert.Equal(trip.EndDate, tripResponse.EndDate);
        Assert.Equal(trip.TicketBookings.Count, tripResponse.TicketBookings.Count);
        Assert.Equal(trip.HotelRoomBookings.Count, tripResponse.HotelRoomBookings.Count);

        await Task.Delay(2000, TestContext.Current.CancellationToken);

        // Act
        response = await tripPlanningHttpClient.GetAsync($"/api/saga/v1/trips/{trip.Id}", TestContext.Current.CancellationToken);
        tripResponse = await response.Content.ReadFromJsonAsync<Trip>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert - now it must be in Confirmed state
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(tripResponse);
        Assert.Equal(TripStatus.Confirmed, tripResponse.Status);
    }

}
