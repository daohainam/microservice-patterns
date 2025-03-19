using Aspire.Hosting;
using Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests;
public class AppFixture : IDisposable
{
    private const int DefaultTimeout = 120;

    public DistributedApplication App => _app;
    private readonly DistributedApplication _app;

    public AppFixture()
    {
        // setup
        var appHost = DistributedApplicationTestingBuilder.CreateAsync<MicroservicePatterns_AppHost>([
            "IsTest=true"
            ]).Result;
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        _app = appHost.BuildAsync().Result;
        _app.StartAsync().Wait();

        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        resourceNotificationService.WaitForResourceAsync<Saga_OnlineStore_CatalogService>(KnownResourceStates.Running).Wait(TimeSpan.FromSeconds(DefaultTimeout));
        resourceNotificationService.WaitForResourceAsync<Saga_OnlineStore_InventoryService>(KnownResourceStates.Running).Wait(TimeSpan.FromSeconds(DefaultTimeout));
        resourceNotificationService.WaitForResourceAsync<Saga_OnlineStore_OrderService>(KnownResourceStates.Running).Wait(TimeSpan.FromSeconds(DefaultTimeout));
        resourceNotificationService.WaitForResourceAsync<Saga_OnlineStore_PaymentService>(KnownResourceStates.Running).Wait(TimeSpan.FromSeconds(DefaultTimeout));

        resourceNotificationService.WaitForResourceAsync<Saga_TripPlanner_HotelService>(KnownResourceStates.Running).Wait(TimeSpan.FromSeconds(DefaultTimeout));
        resourceNotificationService.WaitForResourceAsync<Saga_TripPlanner_TicketService>(KnownResourceStates.Running).Wait(TimeSpan.FromSeconds(DefaultTimeout));
        resourceNotificationService.WaitForResourceAsync<Saga_TripPlanner_PaymentService>(KnownResourceStates.Running).Wait(TimeSpan.FromSeconds(DefaultTimeout));
        resourceNotificationService.WaitForResourceAsync<Saga_TripPlanner_TripPlanningService>(KnownResourceStates.Running).Wait(TimeSpan.FromSeconds(DefaultTimeout));

    }

    public void Dispose()
    {
        if (_app != null)
        {
            _app.Dispose();
        }   
    }
}
