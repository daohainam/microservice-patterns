using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Infrastructure.Models;
public class Event
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string Data { get; set; } = default!;
    public string Metadata { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public Guid StreamId { get; set; }
    public long Version { get; set; }
}
