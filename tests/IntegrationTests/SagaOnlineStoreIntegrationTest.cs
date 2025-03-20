using Aspire.Hosting;
using Microsoft.AspNetCore.Components.Endpoints;
using Saga.OnlineStore.CatalogService.Infrastructure.Entity;
using Saga.OnlineStore.InventoryService.Apis;
using Saga.OnlineStore.InventoryService.Infrastructure.Entity;
using Saga.OnlineStore.OrderService.Infrastructure.Entity;
using Saga.OnlineStore.PaymentService.Apis;
using Saga.OnlineStore.PaymentService.Infrastructure.Entity;
using Saga.TripPlanner.HotelService.Infrastructure.Entity;
using System.Net.Http.Json;

namespace IntegrationTests.Tests
{
    public class SagaOnlineStoreTest : IClassFixture<AppFixture>
    {
        private readonly AppFixture fixture;
        private DistributedApplication App => fixture.App;

        public SagaOnlineStoreTest(AppFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(100, 120, 15, 1500, OrderStatus.Created)]
        [InlineData(100, 15, 15, 1000, OrderStatus.Rejected)] // Not enough balance
        [InlineData(100, 15, 150, 1000, OrderStatus.Rejected)] // Not enough stock
        [InlineData(1000, 120, 15, 10000, OrderStatus.Rejected)] // Not enough balance
        [InlineData(1000, 1200, 1200, 1000 * 1200, OrderStatus.Created)]
        [InlineData(100, 200, 201, 100 * 201, OrderStatus.Rejected)] // Not enough stock
        [InlineData(100, 201, 201, 100 * 201, OrderStatus.Created)]
        public async Task Create_OnlineStore_Order(decimal productPrice, int quantityInStock, int orderItemQuantity, decimal bankAccountBalance, OrderStatus expectedOrderStatus)
        {
            // Arrange

            // Act
            var catalogHttpClient = App.CreateHttpClient<Projects.Saga_OnlineStore_CatalogService>();
            var product = new Product()
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                Price = productPrice,
                Description = "Test Description"
            };
            var response = await catalogHttpClient.PostAsJsonAsync("/api/saga/v1/products", product);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Act
            await Task.Delay(4000);

            var inventoryHttpClient = App.CreateHttpClient<Projects.Saga_OnlineStore_InventoryService>();
            var restockItem = new RestockItem()
            {
                Quantity = quantityInStock
            };
            response = await inventoryHttpClient.PutAsJsonAsync($"/api/saga/v1/inventory/items/{product.Id}/restock", restockItem);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Act
            var inventoryItemResponse = await inventoryHttpClient.GetAsync($"/api/saga/v1/inventory/items/{product.Id}");
            var inventoryItem = await inventoryItemResponse.Content.ReadFromJsonAsync<InventoryItem>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, inventoryItemResponse.StatusCode);
            Assert.NotNull(inventoryItem);
            Assert.Equal(product.Id, inventoryItem.Id);
            Assert.Equal(quantityInStock, inventoryItem.AvailableQuantity);

            await Task.Delay(2000);

            var paymentHttpClient = App.CreateHttpClient<Projects.Saga_OnlineStore_PaymentService>();
            var card = new Card()
            {
                CardNumber = Guid.NewGuid().ToString("N")[..16],
                CardHolderName = "Test Card Holder",
                ExpirationDate = "12/34",
                Cvv = "123"
            };
            response = await paymentHttpClient.PostAsJsonAsync("/api/saga/v1/cards", card);

            // Assert
            card = await response.Content.ReadFromJsonAsync<Card>();
            Assert.NotNull(card);

            // Arrange
            var cardId = card.Id;

            // Act
            response = await paymentHttpClient.PutAsJsonAsync($"/api/saga/v1/cards/{cardId}/deposit", new Deposit()
            {
                Amount = bankAccountBalance
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Act
            response = await paymentHttpClient.GetAsync($"/api/saga/v1/cards/{cardId}");
            card = await response.Content.ReadFromJsonAsync<Card>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(card);
            Assert.Equal(bankAccountBalance, card.Balance);
            Assert.Equal(cardId, card.Id);

            // Act
            var orderHttpClient = App.CreateHttpClient<Projects.Saga_OnlineStore_OrderService>();
            var order = new Order()
            {
                Id = Guid.NewGuid(),
                Items =
                [
                    new OrderItem()
                    {
                        ProductId = product.Id,
                        Quantity = orderItemQuantity,
                        UnitPrice = product.Price
                    }
                ],
                PaymentCardNumber = card.CardNumber,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CustomerPhone = "1234567890",
                ShippingAddress = "Test Address",
                PaymentCardCvv = card.Cvv,
                PaymentCardExpiration = card.ExpirationDate,
                PaymentCardName = card.CardHolderName
            };
            await orderHttpClient.PostAsJsonAsync("/api/saga/v1/orders", order);

            await Task.Delay(2000);

            var orderResponse = await orderHttpClient.GetAsync($"/api/saga/v1/orders/{order.Id}");
            var orderResult = await orderResponse.Content.ReadFromJsonAsync<Order>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, orderResponse.StatusCode);
            Assert.NotNull(orderResult);
            Assert.Equal(order.Id, orderResult.Id);
            Assert.Equal(order.CustomerId, orderResult.CustomerId);
            Assert.Equal(order.CustomerName, orderResult.CustomerName);
            Assert.Equal(order.CustomerPhone, orderResult.CustomerPhone);
            Assert.Equal(order.ShippingAddress, orderResult.ShippingAddress);
            Assert.Equal(order.PaymentCardNumber, orderResult.PaymentCardNumber);
            Assert.Equal(order.PaymentCardName, orderResult.PaymentCardName);
            Assert.Equal(order.PaymentCardExpiration, orderResult.PaymentCardExpiration);
            Assert.Equal(order.PaymentCardCvv, orderResult.PaymentCardCvv);
            Assert.Equal(order.Items.Count, orderResult.Items.Count);
            Assert.Equal(order.Items[0].ProductId, orderResult.Items[0].ProductId);
            Assert.Equal(order.Items[0].Quantity, orderResult.Items[0].Quantity);
            Assert.Equal(expectedOrderStatus, orderResult.Status);

            if (expectedOrderStatus == OrderStatus.Created)
            {
                // assert inventory and balance after order created successfully
                inventoryItemResponse = await inventoryHttpClient.GetAsync($"/api/saga/v1/inventory/items/{product.Id}");
                inventoryItem = await inventoryItemResponse.Content.ReadFromJsonAsync<InventoryItem>();

                Assert.Equal(HttpStatusCode.OK, inventoryItemResponse.StatusCode);
                Assert.NotNull(inventoryItem);
                Assert.Equal(product.Id, inventoryItem.Id);
                Assert.Equal(quantityInStock - orderItemQuantity, inventoryItem.AvailableQuantity);

                var cardResponse = await paymentHttpClient.GetAsync($"/api/saga/v1/cards/{card.Id}");
                var cardResult = await cardResponse.Content.ReadFromJsonAsync<Card>();

                Assert.Equal(HttpStatusCode.OK, cardResponse.StatusCode);
                Assert.NotNull(cardResult);
                Assert.Equal(card.Id, cardResult.Id);
                Assert.Equal(bankAccountBalance - product.Price * orderItemQuantity, cardResult.Balance);
            }
            else
            {
                // assert inventory and balance after order rejected
                inventoryItemResponse = await inventoryHttpClient.GetAsync($"/api/saga/v1/inventory/items/{product.Id}");
                inventoryItem = await inventoryItemResponse.Content.ReadFromJsonAsync<InventoryItem>();

                Assert.Equal(HttpStatusCode.OK, inventoryItemResponse.StatusCode);
                Assert.NotNull(inventoryItem);
                Assert.Equal(product.Id, inventoryItem.Id);
                Assert.Equal(quantityInStock, inventoryItem.AvailableQuantity);

                var cardResponse = await paymentHttpClient.GetAsync($"/api/saga/v1/cards/{card.Id}");
                var cardResult = await cardResponse.Content.ReadFromJsonAsync<Card>();

                Assert.Equal(HttpStatusCode.OK, cardResponse.StatusCode);
                Assert.NotNull(cardResult);
                Assert.Equal(card.Id, cardResult.Id);
                Assert.Equal(bankAccountBalance, cardResult.Balance);
            }
        }
    }
}
