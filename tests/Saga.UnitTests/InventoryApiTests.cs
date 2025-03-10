using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Saga.OnlineStore.IntegrationEvents;
using Saga.OnlineStore.InventoryService;
using Saga.OnlineStore.InventoryService.Apis;
using Saga.OnlineStore.InventoryService.Infrastructure.Data;
using Saga.OnlineStore.InventoryService.Infrastructure.Entity;
using TestHelpers;

namespace Saga.UnitTests
{
    public class InventoryApiTests: IAsyncLifetime
    {
        private SqliteConnection _connection = default!;
        private DbContextOptions<InventoryDbContext> _contextOptions = default!;

        public Task DisposeAsync()
        {
            _connection?.Close();

            return Task.CompletedTask;
        }

        public Task InitializeAsync()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _contextOptions = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseSqlite(_connection)
                .Options;

            return Task.CompletedTask;
        }

        [Fact]
        public async Task Restock_Success()
        {
            // Arrange
            using var context = new InventoryDbContext(_contextOptions);
            context.Database.EnsureCreated();

            var id = Guid.NewGuid();

            context.Items.Add(new InventoryItem()
            {
                Id = id,
                AvailableQuantity = 10
            });
            context.SaveChanges();

            var fakeEventPublisher = new FakeEventPublisher();

            var services = new ApiServices(context, fakeEventPublisher, NullLogger<InventoryApi>.Instance);

            // Act
            await InventoryApi.Restock(services, id, new RestockItem() { 
                Quantity = 55
            });

            // Assert
            var item = context.Items.Find(id);

            Assert.NotNull(item);
            Assert.Equal(65, item.AvailableQuantity);

            Assert.Single(fakeEventPublisher.Events);
            Assert.IsType<ItemQuantityChangedIntegrationEvent>(fakeEventPublisher.Events[0]);
            Assert.Equal(id, ((ItemQuantityChangedIntegrationEvent)fakeEventPublisher.Events[0]).ItemId);
            Assert.Equal(10, ((ItemQuantityChangedIntegrationEvent)fakeEventPublisher.Events[0]).QuantityBefore);
            Assert.Equal(65, ((ItemQuantityChangedIntegrationEvent)fakeEventPublisher.Events[0]).QuantityAfter);
        }

        [Fact]
        public async Task Restock_Failure()
        {
            // Arrange
            using var context = new InventoryDbContext(_contextOptions);
            context.Database.EnsureCreated();
            var id = Guid.NewGuid();
            context.Items.Add(new InventoryItem()
            {
                Id = id,
                AvailableQuantity = 10
            });
            context.SaveChanges();

            var fakeEventPublisher = new FakeEventPublisher();
            var services = new ApiServices(context, fakeEventPublisher, NullLogger<InventoryApi>.Instance);

            // Act
            await InventoryApi.Restock(services, id, new RestockItem()
            {
                Quantity = -5
            });

            // Assert
            var item = context.Items.Find(id);
            Assert.NotNull(item);
            Assert.Equal(10, item.AvailableQuantity);
            Assert.Empty(fakeEventPublisher.Events);
        }

        //[Fact]
        //public async Task ProductIntegrationEvent_Handled_Success()
        //{
        //    // Arrange
        //    using var context = new InventoryDbContext(_contextOptions);
        //    context.Database.EnsureCreated();

        //    var evt = new OrderItemsReservedIntegrationEvent()
        //    {
        //        OrderId = Guid.NewGuid(),
        //        CustomerId = Guid.NewGuid(),
        //        CustomerName = "Test Customer",
        //        CustomerPhone = "1234567890",
        //        Items =
        //        [
        //            new()
        //            {
        //                ProductId = Guid.NewGuid(),
        //                Quantity = 5
        //            },
        //            new()
        //            {
        //                ProductId = Guid.NewGuid(),
        //                Quantity = 10
        //            },
        //            new()
        //            {
        //                ProductId = Guid.NewGuid(),
        //                Quantity = 15
        //            }
        //        ],
        //        PaymentCardNumber = "1234567890123456",
        //        PaymentCardName = "Test Card Holder",
        //        PaymentCardExpiration = "12/23",
        //        PaymentCardCvv = "123",
        //        ShippingAddress = "Test Address"
        //    };


        //    var fakeEventPublisher = new FakeEventPublisher();
        //    var services = new ApiServices(context, fakeEventPublisher);
        //    // Act
        //    var result = await InventoryApi.Create(services, item);
        //    // Assert
        //    Assert.True(result.IsOk);
        //    Assert.Equal(item, result.OkValue);
        //    var createdItem = context.Items.Find(id);
        //    Assert.NotNull(createdItem);
        //    Assert.Equal(item, createdItem);
        //    Assert.Single(fakeEventPublisher.Events);
        //    Assert.IsType<ProductCreatedIntegrationEvent>(fakeEventPublisher.Events[0]);
        //    Assert.Equal(id, ((ProductCreatedIntegrationEvent)fakeEventPublisher.Events[0]).ProductId);
        //}
    }
}
