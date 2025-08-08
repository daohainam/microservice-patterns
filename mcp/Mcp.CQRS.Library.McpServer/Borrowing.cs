namespace Mcp.CQRS.Library.McpServer;
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
