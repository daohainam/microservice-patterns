using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.SeedWork;

/// <summary>
/// Represents a domain event that captures a state change in an aggregate.
/// </summary>
/// <remarks>
/// In Event Sourcing, instead of storing the current state, we store a sequence of events
/// that describe all changes to an aggregate. Domain events are the building blocks of this pattern.
/// </remarks>
public interface IDomainEvent
{
    /// <summary>
    /// Gets or sets the unique identifier for this event.
    /// </summary>
    public Guid EventId { get; set; }
    
    /// <summary>
    /// Gets or sets the version number of the aggregate after this event was applied.
    /// </summary>
    /// <remarks>
    /// Version numbers are used for optimistic concurrency control and to ensure
    /// events are applied in the correct order when rebuilding aggregate state.
    /// </remarks>
    public long Version { get; set; }
    
    /// <summary>
    /// Gets or sets the UTC timestamp when this event occurred.
    /// </summary>
    public DateTime TimestampUtc { get; set; }
}
