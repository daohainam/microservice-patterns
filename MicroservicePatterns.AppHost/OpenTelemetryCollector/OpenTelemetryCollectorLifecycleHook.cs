using Aspire.Hosting.Eventing;
using Microsoft.Extensions.Logging;

namespace MicroservicePatterns.AppHost.OpenTelemetryCollector;

internal sealed class OpenTelemetryCollectorLifecycleHook(ILogger<OpenTelemetryCollectorLifecycleHook> logger)
{
    private const string OtelExporterOtlpEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";

    public void Subscribe(IDistributedApplicationBuilder builder)
    {
        builder.Eventing.Subscribe<ResourceEndpointsAllocatedEvent>((resourceEvent, ct) =>
        {
            if (resourceEvent.Resource is not OpenTelemetryCollectorResource collectorResource)
            {
                return Task.CompletedTask;
            }

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
                    if (context.EnvironmentVariables.ContainsKey(OtelExporterOtlpEndpoint))
                    {
                        logger.LogDebug("Forwarding telemetry for {ResourceName} to the collector ({url}).", resource.Name, endpoint.Url);

                        context.EnvironmentVariables[OtelExporterOtlpEndpoint] = endpoint;
                    }
                }));
            }

            return Task.CompletedTask;
        });
    }
}
