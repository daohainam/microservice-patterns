namespace TransactionalOutbox.Abstractions;
public class LogTailingOutboxMessage
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string Payload { get; set; } = default!;
    public string PayloadType { get; set; } = default!;
}
