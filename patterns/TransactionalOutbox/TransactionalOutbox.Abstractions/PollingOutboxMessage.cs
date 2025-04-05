namespace TransactionalOutbox.Abstractions;
public class PollingOutboxMessage
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string Payload { get; set; } = default!;
    public string PayloadType { get; set; } = default!;
    public DateTime? ProcessedDate { get; set; } // null if not processed, actually we should delete the record after processing but we keep it for now for simplicity
    public int ProcessedCount { get; set; }
}
