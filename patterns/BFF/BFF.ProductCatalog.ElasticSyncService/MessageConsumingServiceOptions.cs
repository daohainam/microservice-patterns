namespace BFF.ProductCatalog.ElasticSyncService;
public class MessageConsumingServiceOptions
{
    public string BootstrapServers { get; set; } = default!;
    public string GroupId { get; set; } = default!;
    public string Topic { get; set; } = default!;

}
