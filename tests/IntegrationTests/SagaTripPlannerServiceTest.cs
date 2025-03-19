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
using System.Net.Http.Json;

namespace IntegrationTests.Tests
{
    public class SagaTripPlannerServiceTest : IClassFixture<AppFixture>
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
            var response = await hotelHttpClient.PostAsJsonAsync("/api/saga/v1/rooms", room);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Act
            var result = await response.Content.ReadFromJsonAsync<Room>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(room.Id, result.Id);
            Assert.Equal(room.Name, result.Name);
            Assert.Equal(room.Price, result.Price);
            Assert.Equal(room.MaxOccupancy, result.MaxOccupancy);

            // Act
            response = await hotelHttpClient.GetAsync($"/api/saga/v1/rooms/{room.Id}");
            result = await response.Content.ReadFromJsonAsync<Room>();

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
            var response = await hotelHttpClient.PostAsJsonAsync("/api/saga/v1/rooms", room);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(150, "train")]
        [InlineData(300, "train")]
        [InlineData(500, "flight")]
        public async Task Create_Ticket_Success(decimal price, string ticketTypeId)
        {
            var ticketHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_TicketService>();
            var ticket = new Ticket()
            {
                Id = Guid.NewGuid(),
                Price = price,
                TicketTypeId = ticketTypeId
            };
            var response = await ticketHttpClient.PostAsJsonAsync("/api/saga/v1/tickets", ticket);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Act
            var result = await response.Content.ReadFromJsonAsync<Ticket>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ticket.Id, result.Id);
            Assert.Equal(ticket.Price, result.Price);
            Assert.Equal(ticket.TicketTypeId, result.TicketTypeId);

            // Act
            response = await ticketHttpClient.GetAsync($"/api/saga/v1/tickets/{ticket.Id}");
            result = await response.Content.ReadFromJsonAsync<Ticket>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal(ticket.Id, result.Id);
            Assert.Equal(ticket.Price, result.Price);
            Assert.Equal(ticket.TicketTypeId, result.TicketTypeId);
        }

        [Theory]
        [InlineData(-150, "train")]
        [InlineData(300, "")]
        [InlineData(0, "train")]
        public async Task Create_Ticket_Failed(decimal price, string ticketTypeId)
        {
            var ticketHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_TicketService>();
            var ticket = new Ticket()
            {
                Id = Guid.NewGuid(),
                Price = price,
                TicketTypeId = ticketTypeId
            };
            var response = await ticketHttpClient.PostAsJsonAsync("/api/saga/v1/tickets", ticket);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(1000, 1000)]
        [InlineData(500, 200)]
        [InlineData(999999, 999999)]
        public async Task Create_And_Use_Credit_Card_Success(decimal creditLimit, decimal borrowAmount)
        {
            var paymentHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_PaymentService>();
            var creditCard = new CreditCard()
            {
                Id = Guid.NewGuid(),
                CardNumber = "1234567890123456",
                CardHolderName = "Test User",
                ExpirationDate = "12/23",
                Cvv = "123",
                CreditLimit = creditLimit,
                AvailableCredit = creditLimit
            };
            var response = await paymentHttpClient.PostAsJsonAsync("/api/saga/v1/payments", creditCard);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Act
            var result = await response.Content.ReadFromJsonAsync<CreditCard>();

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
            response = await paymentHttpClient.GetAsync($"/api/saga/v1/payments/{creditCard.Id}");
            result = await response.Content.ReadFromJsonAsync<CreditCard>();

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

        [Theory]
        [InlineData(-1000, "Credit Card")]
        [InlineData(500, "")]
        [InlineData(0, "Bank Transfer")]
        public async Task Create_Payment_Failed(decimal amount, string method)
        {
            var paymentHttpClient = App.CreateHttpClient<Projects.Saga_TripPlanner_PaymentService>();
            var payment = new CreditCard()
            {
                Id = Guid.NewGuid(),
            };
            var response = await paymentHttpClient.PostAsJsonAsync("/api/saga/v1/payments", payment);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


    }
}
