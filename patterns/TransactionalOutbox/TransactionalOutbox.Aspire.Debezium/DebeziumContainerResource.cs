using Aspire.Hosting.ApplicationModel;

namespace TransactionalOutbox.Aspire.Debezium;
public sealed partial class DebeziumContainerResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? _primaryEndpoint;

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