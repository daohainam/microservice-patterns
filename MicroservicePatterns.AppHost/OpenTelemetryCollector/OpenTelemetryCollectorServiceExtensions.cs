using Aspire.Hosting.Eventing;

namespace MicroservicePatterns.AppHost.OpenTelemetryCollector;

internal static class OpenTelemetryCollectorServiceExtensions
{
    public static IDistributedApplicationBuilder AddOpenTelemetryCollectorInfrastructure(this IDistributedApplicationBuilder builder)
    {
        builder.Eventing.Subscribe<ResourceEndpointsAllocatedEvent>((resourceEvent, ct) =>
        {
            if (resourceEvent.Resource is not OpenTelemetryCollectorResource collectorResource)
            {
                return Task.CompletedTask;
            }

            var logger = resourceEvent.Services.GetRequiredService<ILoggerFactory>()
                .CreateLogger("MicroservicePatterns.AppHost.OpenTelemetryCollector");
            var endpoint = collectorResource.GetEndpoint(OpenTelemetryCollectorResource.OtlpGrpcEndpointName);
            if (!endpoint.Exists)
            {
                logger.LogWarning("No {EndpointName} endpoint for the collector.", OpenTelemetryCollectorResource.OtlpGrpcEndpointName);
                return Task.CompletedTask;
            }

            var appModel = resourceEvent.Services.GetRequiredService<DistributedApplicationModel>();
            foreach (var resource in appModel.Resources)
            {
                resource.Annotations.Add(new EnvironmentCallbackAnnotation((context) =>
                {
                    if (context.EnvironmentVariables.ContainsKey("OTEL_EXPORTER_OTLP_ENDPOINT"))
                    {
                        logger.LogDebug("Forwarding telemetry for {ResourceName} to the collector ({url}).", resource.Name, endpoint.Url);

                        context.EnvironmentVariables["OTEL_EXPORTER_OTLP_ENDPOINT"] = endpoint;
                    }
                }));
            }

            return Task.CompletedTask;
        });

        return builder;
    }
}
