using Aspire.Hosting;
using Microsoft.AspNetCore.Components.Endpoints;
using Saga.OnlineStore.CatalogService.Infrastructure.Entity;
using Saga.OnlineStore.InventoryService.Apis;
using Saga.OnlineStore.InventoryService.Infrastructure.Entity;
using Saga.OnlineStore.OrderService.Infrastructure.Entity;
using Saga.OnlineStore.PaymentService.Apis;
using Saga.OnlineStore.PaymentService.Infrastructure.Entity;
using Saga.TripPlanner.HotelService.Infrastructure.Entity;
using Saga.TripPlanner.PaymentService.Infrastructure.Entity;
using Saga.TripPlanner.TicketService.Infrastructure.Entity;
using Saga.TripPlanner.TripPlanningService.Infrastructure.Entity;
using System.Net.Http.Json;

namespace IntegrationTests.Tests
{
    public class SagaTripPlannerIntegrationTest : IClassFixture<AppFixture>
    {
        private readonly AppFixture fixture;
        private DistributedApplication App => fixture.App;

        public SagaTripPlannerIntegrationTest(AppFixture fixture)
        {
            this.fixture = fixture;
        }


        [Theory]
        [InlineData(100, 2, "flight", 200)]
        [InlineData(1000, 20, "flight", 200)]
        [InlineData(10000, 20, "train_100", 10000 + 100)]
        [InlineData(100000, 20, "flight", 100000 + 200)]
        [InlineData(9999, 1, "bus", 9999 + 50)]
        public async Task Create_Create_TripPlanner_Success(decimal price, int maxOccupancy, string ticketTypeId, decimal creditLimit)
        {
            var hotelHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_HotelService>();
            var room = new Room()
            {
                Id = Guid.NewGuid(),
                Name = "Test Room",
                Price = price,
                MaxOccupancy = maxOccupancy
            };
            var response = await hotelHttpClient.PostAsJsonAsync("/api/saga/v1/rooms", room);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Act
            var ticketHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_TicketService>();

            response = await ticketHttpClient.PostAsJsonAsync("/api/saga/v1/ticket-types", new TicketType()
            {
                Id = ticketTypeId,
                Name = $"Test Ticket: {ticketTypeId}",
                Price = price,
                AvailableTickets = 1
            });

            // Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Conflict);

            // Act
            var ticket = new Ticket()
            {
                Id = Guid.NewGuid(),
                TicketTypeId = ticketTypeId
            };
            response = await ticketHttpClient.PostAsJsonAsync("/api/saga/v1/tickets", ticket);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

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
            response = await paymentHttpClient.PostAsJsonAsync("/api/saga/v1/cards", creditCard);

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
                    }
                ],
                HotelRoomBookings =
                [
                    new()
                    {
                        Id = Guid.NewGuid(),
                        BookingDate = DateTime.UtcNow,
                    }
                ]
            };

            response = await tripPlanningHttpClient.PostAsJsonAsync("/api/saga/v1/trips", trip);
            var tripResponse = await response.Content.ReadFromJsonAsync<Trip>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(tripResponse);
            Assert.Equal(trip.Name, tripResponse.Name);
            Assert.Equal(trip.StartDate, tripResponse.StartDate);
            Assert.Equal(trip.EndDate, tripResponse.EndDate);
            Assert.Equal(trip.TicketBookings.Count, tripResponse.TicketBookings.Count);
            Assert.Equal(trip.HotelRoomBookings.Count, tripResponse.HotelRoomBookings.Count);

            await Task.Delay(2000);
        }

    }
}
