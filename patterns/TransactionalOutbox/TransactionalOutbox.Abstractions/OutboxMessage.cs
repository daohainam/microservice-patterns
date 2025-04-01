namespace TransactionalOutbox.Abstractions;
public class OutboxMessage
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string Message { get; set; } = default!;
    public DateTime? ProcessedDate { get; set; }
    public int ProcessedCount { get; set; }
}
