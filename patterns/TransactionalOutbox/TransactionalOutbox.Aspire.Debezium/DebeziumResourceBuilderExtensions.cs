using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Eventing;

namespace TransactionalOutbox.Aspire.Debezium;

public static class DebeziumBuilderExtensions
{
    private const int DebeziumConnectPort = 8083;

    public static IResourceBuilder<DebeziumContainerResource> AddDebezium(this IDistributedApplicationBuilder builder,
        Action<IResourceBuilder<DebeziumContainerResource>>? configureContainer = null,
        string? name = null,
        int? port = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Resources.OfType<DebeziumContainerResource>().SingleOrDefault() is { } existingDebeziumContainerResource)
        {
            var builderForExistingResource = builder.CreateResourceBuilder(existingDebeziumContainerResource);
            configureContainer?.Invoke(builderForExistingResource);
            return builderForExistingResource;
        }
        else
        {
            name ??= "debezium";
            port ??= DebeziumConnectPort;

            var debezium = new DebeziumContainerResource(name);
            var debeziumBuilder = builder.AddResource(debezium)
                .WithImage(DebeziumContainerImageTags.Image, DebeziumContainerImageTags.Tag)
                .WithImageRegistry(DebeziumContainerImageTags.Registry)
                .WithHttpEndpoint(targetPort: DebeziumConnectPort, port: port)
                .WithEnvironment("GROUP_ID", "1")
                .WithEnvironment("CONFIG_STORAGE_TOPIC", "my_connect_configs")
                .WithEnvironment("OFFSET_STORAGE_TOPIC", "my_connect_offsets")
                .WithEnvironment("STATUS_STORAGE_TOPIC", "my_connect_status")
                .WithEnvironment("CONNECT_KEY_CONVERTER", "org.apache.kafka.connect.json.JsonConverter")
                .WithEnvironment("CONNECT_VALUE_CONVERTER", "org.apache.kafka.connect.json.JsonConverter")
                .WithEnvironment("CONNECT_INTERNAL_KEY_CONVERTER", "org.apache.kafka.connect.json.JsonConverter")
                .WithEnvironment("CONNECT_INTERNAL_VALUE_CONVERTER", "org.apache.kafka.connect.json.JsonConverter")
                .WithEnvironment("CONNECT_REST_ADVERTISED_HOST_NAME", "connect")
                .WithEnvironment("CONNECT_PLUGIN_PATH", "/kafka/connect,/usr/share/java");

            builder.Eventing.Subscribe<ResourceEndpointsAllocatedEvent>((e, ct) =>
            {
                var kafkaResources = builder.Resources.OfType<KafkaServerResource>();

                int i = 0;
                foreach (var kafkaResource in kafkaResources)
                {
                    if (kafkaResource.InternalEndpoint.IsAllocated)
                    {
                        var endpoint = kafkaResource.InternalEndpoint;
                        int index = i;
                        debeziumBuilder.WithEnvironment("BOOTSTRAP_SERVERS", kafkaResource.InternalEndpoint);
                    }

                    i++;
                }

                return Task.CompletedTask;
            });

            configureContainer?.Invoke(debeziumBuilder);

            return debeziumBuilder;
        }
    }
}

internal static class DebeziumContainerImageTags
{
    internal const string Registry = "docker.io";

    internal const string Image = "debezium/connect";

    internal const string Tag = "3.0.0.Final";
}
