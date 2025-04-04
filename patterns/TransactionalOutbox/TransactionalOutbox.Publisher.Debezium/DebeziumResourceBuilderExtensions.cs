using Aspire.Hosting;

namespace TransactionalOutbox.Publisher.Debezium;

public static class DebeziumBuilderExtensions
{
    private const int DebeziumConnectPort = 8083;

    public static IResourceBuilder<KafkaServerResource> WithDebezium(this IResourceBuilder<KafkaServerResource> builder, Action<IResourceBuilder<DebeziumContainerResource>>? configureContainer = null, string? containerName = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.ApplicationBuilder.Resources.OfType<DebeziumContainerResource>().SingleOrDefault() is { } existingDebeziumContainerResource)
        {
            var builderForExistingResource = builder.ApplicationBuilder.CreateResourceBuilder(existingDebeziumContainerResource);
            configureContainer?.Invoke(builderForExistingResource);
            return builder;
        }
        else
        {
            containerName ??= $"{builder.Resource.Name}-debezium";

            var debezium = new DebeziumContainerResource(containerName);
            var debeziumBuilder = builder.ApplicationBuilder.AddResource(debezium)
                .WithImage(DebeziumContainerImageTags.Image, DebeziumContainerImageTags.Tag)
                .WithImageRegistry(DebeziumContainerImageTags.Registry)
                .WithEndpoint(targetPort: DebeziumConnectPort)
                .WaitFor(builder)
                .WithParentRelationship(builder)
                .ExcludeFromManifest();

            builder.ApplicationBuilder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>((e, ct) =>
            {
                var kafkaResources = builder.ApplicationBuilder.Resources.OfType<KafkaServerResource>();

                int i = 0;
                foreach (var kafkaResource in kafkaResources)
                {
                    if (kafkaResource.InternalEndpoint.IsAllocated)
                    {
                        var endpoint = kafkaResource.InternalEndpoint;
                        int index = i;
                        debeziumBuilder.WithEnvironment(context => ConfigureDebeziumContainer(context, endpoint, index));
                    }

                    i++;
                }
                return Task.CompletedTask;
            });

            configureContainer?.Invoke(debeziumBuilder);

            return builder;
        }
    }

    private static void ConfigureDebeziumContainer(EnvironmentCallbackContext context, EndpointReference endpoint, int index)
    {
        var bootstrapServers = context.ExecutionContext.IsRunMode
            ? ReferenceExpression.Create($"{endpoint.Resource.Name}:{endpoint.Property(EndpointProperty.TargetPort)}")
            : ReferenceExpression.Create($"{endpoint.Property(EndpointProperty.Host)}:{endpoint.Property(EndpointProperty.Port)}");

        context.EnvironmentVariables.Add("BOOTSTRAP_SERVERS", bootstrapServers);
        context.EnvironmentVariables.Add("GROUP_ID", $"{index}");
        context.EnvironmentVariables.Add("CONFIG_STORAGE_TOPIC", "my_connect_configs");
        context.EnvironmentVariables.Add("OFFSET_STORAGE_TOPIC", "my_connect_offsets");
        context.EnvironmentVariables.Add("STATUS_STORAGE_TOPIC", "my_connect_status");
        context.EnvironmentVariables.Add("CONNECT_KEY_CONVERTER", "org.apache.kafka.connect.json.JsonConverter");
        context.EnvironmentVariables.Add("CONNECT_VALUE_CONVERTER", "org.apache.kafka.connect.json.JsonConverter");
        context.EnvironmentVariables.Add("CONNECT_INTERNAL_KEY_CONVERTER", "org.apache.kafka.connect.json.JsonConverter");
        context.EnvironmentVariables.Add("CONNECT_INTERNAL_VALUE_CONVERTER", "org.apache.kafka.connect.json.JsonConverter");
        context.EnvironmentVariables.Add("CONNECT_REST_ADVERTISED_HOST_NAME", "connect");
        context.EnvironmentVariables.Add("CONNECT_PLUGIN_PATH", "/kafka/connect,/usr/share/java");


    }
}

internal static class DebeziumContainerImageTags
{
    internal const string Registry = "docker.io";

    internal const string Image = "debezium/connect";

    internal const string Tag = "3.0.0.Final";
}
