using System.Text.Json.Serialization;

namespace Saga.TripPlanner.TicketService.Infrastructure.Entity;
public class Ticket
{
    public Guid Id { get; set; }
    public string TicketTypeId { get; set; } = default!;
    public decimal Price { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Pending;
}

public enum TicketStatus
{
    Pending,
    Rejected,
    Confirmed,
    Cancelled
}