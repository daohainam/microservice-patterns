namespace CQRS.Library.BorrowingHistoryService.Infrastructure.Entity
{
    [Index(nameof(BorrowerId), nameof(BookId), nameof(ValidUntil), nameof(HasReturned))]
    public class BorrowingHistoryItem
    {
        public Guid Id { get; set; }
        public Guid BorrowerId { get; set; }
        public Guid BookId { get; set; }
        public DateTime BorrowedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool HasReturned { get; set; }
        public string BorrowerName { get; set; } = default!;
        public string BorrowerAddress { get; set; } = default!;
        public string BorrowerEmail { get; set; } = default!;
        public string BorrowerPhoneNumber { get; set; } = default!;
        public string BookTitle { get; set; } = default!;
        public string BookAuthor { get; set; } = default!;
    }
}
