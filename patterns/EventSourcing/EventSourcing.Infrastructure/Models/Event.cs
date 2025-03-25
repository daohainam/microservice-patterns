namespace EventSourcing.Infrastructure.Models;
public class Event
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string Data { get; set; } = default!;
    public string Metadata { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
    public Guid StreamId { get; set; }
    public long Version { get; set; }
}
