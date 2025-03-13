using System.Text.Json.Serialization;

namespace Saga.OnlineStore.PaymentService.Infrastructure.Entity;
public class Card
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; } = default!;
    public string CardHolderName { get; set; } = default!;
    public string ExpirationDate { get; set; } = default!;
    public string Cvv { get; set; } = default!;
    public decimal Balance { get; set; }
}
