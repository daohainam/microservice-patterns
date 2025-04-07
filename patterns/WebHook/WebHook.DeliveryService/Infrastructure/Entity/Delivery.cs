namespace WebHook.DeliveryService.Infrastructure.Entity
{
    public class Delivery
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
    }
}
