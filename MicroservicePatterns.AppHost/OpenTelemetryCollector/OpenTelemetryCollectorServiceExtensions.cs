using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Logging;

namespace MicroservicePatterns.AppHost.OpenTelemetryCollector;

internal static class OpenTelemetryCollectorServiceExtensions
{
    private const string LoggerCategory = "MicroservicePatterns.AppHost.OpenTelemetryCollector";
    private const string OtelExporterOtlpEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";

    public static IDistributedApplicationBuilder AddOpenTelemetryCollectorInfrastructure(this IDistributedApplicationBuilder builder)
    {
        builder.Eventing.Subscribe<ResourceEndpointsAllocatedEvent>((e, ct) =>
        {
            // Check if this is the OpenTelemetry collector resource
            if (e.Resource is OpenTelemetryCollectorResource collectorResource)
            {
                var endpoint = collectorResource.GetEndpoint(OpenTelemetryCollectorResource.OtlpGrpcEndpointName);
                if (!endpoint.Exists)
                {
                    return Task.CompletedTask;
                }

                var logger = e.Services.GetRequiredService<ILoggerFactory>()
                    .CreateLogger(LoggerCategory);

                // Configure all resources to use this collector
                // We need to iterate through resources from the builder since ResourceEndpointsAllocatedEvent doesn't expose the model
                foreach (var resource in builder.Resources)
                {
                    resource.Annotations.Add(new EnvironmentCallbackAnnotation((context) =>
                    {
                        if (context.EnvironmentVariables.ContainsKey(OtelExporterOtlpEndpoint))
                        {
                            logger.LogDebug("Forwarding telemetry for {ResourceName} to the collector ({url}).", resource.Name, endpoint.Url);

                            context.EnvironmentVariables[OtelExporterOtlpEndpoint] = endpoint;
                        }
                    }));
                }
            }

            return Task.CompletedTask;
        });

        return builder;
    }
}
