namespace IdempotentConsumer.CatalogService.Infrastructure.Entity;
[Index(nameof(ProcessedAtUtc))]
public class ProcessedMessage
{
    public Guid Id { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
