using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.SeedWork;
public interface IDomainEvent
{
    public Guid EventId { get; set; }
    public long Version { get; set; }
    public DateTime TimestampUtc { get; set; }
}
