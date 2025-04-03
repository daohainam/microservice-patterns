using Aspire.Hosting;

namespace TransactionalOutbox.Publisher.Debezium
{
    public static class DebeziumResourceBuilderExtensions
    {
        public static IResourceBuilder<DebeziumResource> AddEventStore(
        this IDistributedApplicationBuilder builder,
        string name,
        int? httpPort = null,
        int? tcpPort = null)
        {
            var resource = new DebeziumResource(name);

            return builder.AddResource(resource)
                          .WithImage(DebeziumContainerImageTags.Image)
                          .WithImageRegistry(DebeziumContainerImageTags.Registry)
                          .WithImageTag(DebeziumContainerImageTags.Tag)
                          .WithEndpoint(port: tcpPort, targetPort: DebeziumResource.DefaultTcpPort, name: DebeziumResource.TcpEndpointName)
                          .WithHttpEndpoint(port: httpPort, targetPort: DebeziumResource.DefaultHttpPort, name: DebeziumResource.HttpEndpointName)
                          .WithEnvironment(ConfigureDebeziumContainer);
        }

        private static void ConfigureDebeziumContainer(EnvironmentCallbackContext context)
        {
        }

        public static IResourceBuilder<DebeziumResource> WithKafka(this IResourceBuilder<DebeziumResource> builder, string server)
        {
            return builder
                .WithEnvironment("KAFKA_CONNECT_BOOTSTRAP_SERVERS", server);
        }

        public static IResourceBuilder<DebeziumResource> WithDataVolume(this IResourceBuilder<DebeziumResource> builder)
        {
            return builder
                .WithVolume("debezium-volume-data", "/var/lib/debezium-data");
        }
        public static IResourceBuilder<DebeziumResource> WithLogsVolume(this IResourceBuilder<DebeziumResource> builder)
        {
            return builder
                .WithVolume("debezium-volume-logs", "/var/log/debezium")
                ;
        }
    }

    internal static class DebeziumContainerImageTags
    {
        internal const string Registry = "quay.io";

        internal const string Image = "debezium/connect";

        internal const string Tag = "latest";
    }
}
