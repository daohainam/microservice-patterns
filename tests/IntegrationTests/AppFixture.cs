using Aspire.Hosting;
using IntegrationTests;
using Projects;

[assembly: AssemblyFixture(typeof(AppFixture))]

namespace IntegrationTests;
public class AppFixture : IAsyncLifetime
{
    private const int DefaultTimeout = 120;

    public DistributedApplication App => _app!;
    private DistributedApplication? _app;

    public async ValueTask InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<MicroservicePatterns_AppHost>([
            "IsTest=true"
        ]);

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService.WaitForResourceAsync<Saga_OnlineStore_CatalogService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(DefaultTimeout));
        await resourceNotificationService.WaitForResourceAsync<Saga_OnlineStore_InventoryService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(DefaultTimeout));
        await resourceNotificationService.WaitForResourceAsync<Saga_OnlineStore_OrderService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(DefaultTimeout));
        await resourceNotificationService.WaitForResourceAsync<Saga_OnlineStore_PaymentService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(DefaultTimeout));

        await resourceNotificationService.WaitForResourceAsync<Saga_TripPlanner_HotelService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(DefaultTimeout));
        await resourceNotificationService.WaitForResourceAsync<Saga_TripPlanner_TicketService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(DefaultTimeout));
        await resourceNotificationService.WaitForResourceAsync<Saga_TripPlanner_PaymentService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(DefaultTimeout));
        await resourceNotificationService.WaitForResourceAsync<Saga_TripPlanner_TripPlanningService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(DefaultTimeout));

        await Task.Delay(2000);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }
}
