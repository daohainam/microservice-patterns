namespace Saga.TripPlanner.PaymentService.Infrastructure.Entity;
public class CreditCard
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; } = default!;
    public string CardHolderName { get; set; } = default!;
    public string ExpirationDate { get; set; } = default!;
    public string Cvv { get; set; } = default!;
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
}
