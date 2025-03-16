namespace Saga.TripPlanner.TicketService.Infrastructure.Entity
{
    public class TicketType
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public int AvailableTickets { get; set; } = default!;
        public List<Ticket> Tickets { get; set; } = default!;
    }
}
