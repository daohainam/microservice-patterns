namespace TransactionalOutbox.Publisher.Debezium
{
    public sealed class DebeziumResource(string name) : ContainerResource(name), IResourceWithConnectionString
    {
        internal const string HttpEndpointName = "http";
        internal const string TcpEndpointName = "tcp";
        internal const int DefaultHttpPort = 2113;
        internal const int DefaultTcpPort = 1113;

        // An EndpointReference is a core .NET Aspire type used for keeping
        // track of endpoint details in expressions. Simple literal values cannot
        // be used because endpoints are not known until containers are launched.
        private EndpointReference? _httpEndPointReference;

        public EndpointReference DebeziumEndpoint =>
            _httpEndPointReference ??= new(this, HttpEndpointName);
        public ReferenceExpression ConnectionStringExpression =>
            ReferenceExpression.Create(
                $"esdb://{DebeziumEndpoint.Property(EndpointProperty.Host)}:{DebeziumEndpoint.Property(EndpointProperty.Port)}?tls=false"
            );
    }
}
