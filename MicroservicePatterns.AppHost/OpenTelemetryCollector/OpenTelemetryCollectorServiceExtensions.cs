using Aspire.Hosting.Eventing;

namespace MicroservicePatterns.AppHost.OpenTelemetryCollector;

internal static class OpenTelemetryCollectorServiceExtensions
{
    public static IDistributedApplicationBuilder AddOpenTelemetryCollectorInfrastructure(this IDistributedApplicationBuilder builder)
    {
        var hook = new OpenTelemetryCollectorLifecycleHook(
            builder.Services.BuildServiceProvider().GetRequiredService<ILogger<OpenTelemetryCollectorLifecycleHook>>());
        hook.Subscribe(builder);

        return builder;
    }
}
