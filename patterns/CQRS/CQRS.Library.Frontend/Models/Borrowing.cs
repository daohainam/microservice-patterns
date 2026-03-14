namespace CQRS.Library.Frontend.Models;

public class Borrowing
{
    public Guid Id { get; set; }
    public Guid BorrowerId { get; set; }
    public Guid BookId { get; set; }
    public DateTime BorrowedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool HasReturned { get; set; }
}

public class BorrowingRequest
{
    public Guid BorrowerId { get; set; }
    public Guid BookId { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class BorrowingHistoryItem
{
    public Guid Id { get; set; }
    public Guid BorrowerId { get; set; }
    public Guid BookId { get; set; }
    public DateTime BorrowedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool HasReturned { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public string BorrowerAddress { get; set; } = string.Empty;
    public string BorrowerEmail { get; set; } = string.Empty;
    public string BorrowerPhoneNumber { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
}

public class PaginatedResult<T>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public long Count { get; set; }
    public IEnumerable<T> Items { get; set; } = [];
}
