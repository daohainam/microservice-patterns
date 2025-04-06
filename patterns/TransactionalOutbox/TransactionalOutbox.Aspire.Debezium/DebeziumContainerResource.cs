namespace TransactionalOutbox.Aspire.Debezium;
public sealed partial class DebeziumContainerResource : ContainerResource, IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? _primaryEndpoint;

    public DebeziumContainerResource(string name) : base(name) { }

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    public ReferenceExpression ConnectionStringExpression {
        get
        {
            var builder = new ReferenceExpressionBuilder();
            builder.Append($"{PrimaryEndpoint.Property(EndpointProperty.Url)}");

            return builder.Build();
        }
    }
}