
namespace Mcp.CQRS.Library.McpServer;
public interface ILibraryService
{
    Task<IEnumerable<Book>> GetBooks(CancellationToken cancellationToken = default);
    Task<Book> GetBookById(Guid bookId, CancellationToken cancellationToken);
    Task<Book> CreateBook(Book book, CancellationToken cancellationToken = default);
    Task UpdateBook(Guid bookId, Book book, CancellationToken cancellationToken = default);
    Task<IEnumerable<Borrower>> GetBorrowers(CancellationToken cancellationToken = default);
    Task<Borrower> GetBorrowerById(Guid borrowerId, CancellationToken cancellationToken);
    Task<Borrower> CreateBorrower(Borrower borrower, CancellationToken cancellationToken = default);
    Task UpdateBorrower(Guid borrowerId, Borrower borrower, CancellationToken cancellationToken = default);
    Task<Borrowing> GetBorrowingById(Guid borrowerId, CancellationToken cancellationToken);
    Task<IEnumerable<Borrowing>> GetBorrowings(CancellationToken cancellationToken = default);
    Task<Borrowing> CreateBorrowing(Borrowing borrowing, CancellationToken cancellationToken = default);
    Task ReturnBook(Guid borrowingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BorrowingHistoryItem>> GetBorrowingHistoryItems(Guid? borrowerId, Guid? bookId, string query = "", CancellationToken cancellationToken = default);
}
