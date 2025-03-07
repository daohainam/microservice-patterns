using System.Text.Json.Serialization;

namespace Saga.OnlineStore.PaymentService.Infrastructure.Entity
{
    public class Card
    {
        public Guid Id { get; set; }
        public string CardNumber { get; set; } = default!;
        public string CardHolderName { get; set; } = default!;
        public DateTime ExpirationDate { get; set; }
        public string Cvv { get; set; } = default!;
        [JsonIgnore]
        public decimal Balance { get; set; }
    }
}
