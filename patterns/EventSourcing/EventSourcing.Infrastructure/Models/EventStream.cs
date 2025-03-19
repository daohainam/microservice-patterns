using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Infrastructure.Models;
public class EventStream
{
    public Guid Id { get; set; }
    public long CurrentVersion { get; set; }
}
