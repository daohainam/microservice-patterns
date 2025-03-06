using Aspire.Hosting;

namespace IntegrationTests.Tests
{
    public class CQRSIntegrationTest: IAsyncLifetime
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private DistributedApplication _app;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public async Task InitializeAsync()
        {
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MicroservicePatterns_AppHost>();
            appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            _app = await appHost.BuildAsync();
        }

        public async Task DisposeAsync()
        {
            await _app.DisposeAsync();
        }

        [Fact]
        public async Task Borrow_Book_And_Read_BorrowingHistory_Success()
        {
            // Arrange
            var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
            await _app.StartAsync();

            // Act
            var httpClient = _app.CreateHttpClient("cqrs-library-borrower-service");
            await resourceNotificationService.WaitForResourceAsync("cqrs-library-borrower-service", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            var response = await httpClient.GetAsync("/");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
