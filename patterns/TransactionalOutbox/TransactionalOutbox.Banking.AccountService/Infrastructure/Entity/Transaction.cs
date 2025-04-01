namespace TransactionalOutbox.Banking.AccountService.Infrastructure.Entity
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; } // positive for deposit, negative for withdrawal
        public DateTime TimeStamp { get; set; }
    }
}
