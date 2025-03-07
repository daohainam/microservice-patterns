using Aspire.Hosting;
using MicroservicePatterns.Shared.Pagination;
using Saga.OnlineStore.CatalogService.Infrastructure.Entity;
using Saga.OnlineStore.InventoryService.Apis;
using Saga.OnlineStore.OrderService.Infrastructure.Entity;
using Saga.OnlineStore.PaymentService.Apis;
using Saga.OnlineStore.PaymentService.Infrastructure.Entity;
using System.Net.Http.Json;

namespace IntegrationTests.Tests
{
    public class SagaIntegrationTest: IAsyncLifetime
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private DistributedApplication _app;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public async Task InitializeAsync()
        {
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MicroservicePatterns_AppHost>([
                "IsTest=true"
                ]);
            appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            _app = await appHost.BuildAsync();
            await _app.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _app.DisposeAsync();
        }

        [Fact]
        public async Task Create_Order_Success()
        {
            // Arrange
            var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
            await resourceNotificationService.WaitForResourceAsync<Projects.Saga_OnlineStore_CatalogService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            await resourceNotificationService.WaitForResourceAsync<Projects.Saga_OnlineStore_InventoryService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            await resourceNotificationService.WaitForResourceAsync<Projects.Saga_OnlineStore_OrderService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            await resourceNotificationService.WaitForResourceAsync<Projects.Saga_OnlineStore_PaymentService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));

            // Act
            var catalogHttpClient = _app.CreateHttpClient<Projects.Saga_OnlineStore_CatalogService>();
            var product = new Product()
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                Price = 100,
                Description = "Test Description"
            };
            var response = await catalogHttpClient.PostAsJsonAsync("/api/saga/v1/products", product);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Act
            await Task.Delay(1000);

            var inventoryHttpClient = _app.CreateHttpClient<Projects.Saga_OnlineStore_InventoryService>();
            var restockItem = new RestockItem()
            {
                Quantity = 120
            };
            await inventoryHttpClient.PutAsJsonAsync($"/api/saga/v1/inventory/items/{product.Id}/restock", restockItem);

            await Task.Delay(1000);

            var paymentHttpClient = _app.CreateHttpClient<Projects.Saga_OnlineStore_PaymentService>();
            var card = new Card()
            {
                CardNumber = "1234567890123456",
                CardHolderName = "Test Card Holder",
                ExpirationDate = DateTime.Today.AddYears(1).ToUniversalTime(),
                Cvv = "123"
            };
            response = await paymentHttpClient.PostAsJsonAsync("/api/saga/v1/cards", card);

            // Assert
            card = await response.Content.ReadFromJsonAsync<Card>();
            Assert.NotNull(card);

            // Act
            response = await paymentHttpClient.PutAsJsonAsync($"/api/saga/v1/cards/{card.Id}/deposit", new Deposit() 
            {
                Amount = 8000
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Act
            var orderHttpClient = _app.CreateHttpClient<Projects.Saga_OnlineStore_OrderService>();
            var order = new Order()
            {
                Id = Guid.NewGuid(),
                Items =
                [
                    new OrderItem()
                    {
                        ProductId = product.Id,
                        Quantity = 15
                    }
                ],
                PaymentCardNumber = card.CardNumber,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CustomerPhone = "1234567890",
                ShippingAddress = "Test Address",
                PaymentCardCvv = card.Cvv,
                PaymentCardExpiration = card.ExpirationDate.ToString("MM/yy"),
                PaymentCardName = card.CardHolderName
            };
            await orderHttpClient.PostAsJsonAsync("/api/saga/v1/orders", order);

            await Task.Delay(5000);

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
            Assert.Equal(OrderStatus.Created, order.Status);
        }
    }
}
