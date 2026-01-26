namespace IdempotentConsumer.CatalogService.Infrastructure.Entity;

[Index(nameof(ProcessedAtUtc))]
[Index(nameof(Id), IsUnique = true)]
public class ProcessedMessage
{
    public Guid Id { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
