namespace WebHook.DeliveryService.Infrastructure.Entity
{
    public class Delivery
    {
        public Guid Id { get; set; }
        public string Sender { get; set; } = default!;
        public string Receiver { get; set; } = default!;
        public string SenderAddress { get; set; } = default!;
        public string ReceiverAddress { get; set; } = default!;
        public string PackageInfo { get; set; } = default!;
    }
}
