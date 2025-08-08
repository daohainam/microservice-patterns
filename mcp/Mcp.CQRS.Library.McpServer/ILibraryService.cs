
namespace Mcp.CQRS.Library.McpServer;
public interface ILibraryService
{
    Task<IEnumerable<Book>> GetBooks(CancellationToken cancellationToken = default);
    Task<Book> GetBookById(Guid bookId, CancellationToken cancellationToken);
    Task<IEnumerable<Borrower>> GetBorrowers(CancellationToken cancellationToken = default);
    Task<Borrower> GetBorrowerById(Guid borrowerId, CancellationToken cancellationToken);
    Task<Borrowing> GetBorrowingById(Guid borrowerId, CancellationToken cancellationToken);
    Task<IEnumerable<Borrowing>> GetBorrowings(CancellationToken cancellationToken = default);
}
