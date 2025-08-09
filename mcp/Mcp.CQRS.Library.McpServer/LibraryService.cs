
using System.Text;

namespace Mcp.CQRS.Library.McpServer;
public class LibraryService(HttpClient bookHttpClient, HttpClient borrowerHttpClient, HttpClient borrowingHttpClient, HttpClient borrowingHistoryHttpClient) : ILibraryService
{
    public async Task<Book> GetBookById(Guid bookId, CancellationToken cancellationToken)
    {
        var bookResponse = await bookHttpClient.GetFromJsonAsync<Book>($"/api/cqrs/v1/books/{bookId}", cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve book from the book service.");
        return bookResponse;
    }

    public async Task<IEnumerable<Book>> GetBooks(CancellationToken cancellationToken = default)
    {
        var bookResponse = await bookHttpClient.GetFromJsonAsync<IEnumerable<Book>>("/api/cqrs/v1/books", cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve books from the book service.");
        return bookResponse;
    }

    public async Task<Borrower> GetBorrowerById(Guid borrowerId, CancellationToken cancellationToken)
    {
        var bookResponse = await borrowerHttpClient.GetFromJsonAsync<Borrower>($"/api/cqrs/v1/borrowers/{borrowerId}", cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve borrower from the borrower service.");
        return bookResponse;
    }

    public async Task<IEnumerable<Borrower>> GetBorrowers(CancellationToken cancellationToken = default)
    {
        var bookResponse = await borrowerHttpClient.GetFromJsonAsync<IEnumerable<Borrower>>("/api/cqrs/v1/borrowers", cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve borrowers from the borrower service.");
        return bookResponse;
    }

    public async Task<Borrowing> GetBorrowingById(Guid borrowerId, CancellationToken cancellationToken)
    {
        var bookResponse = await borrowingHttpClient.GetFromJsonAsync<Borrowing>($"/api/cqrs/v1/borrowings/{borrowerId}", cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve borrower from the borrower service.");
        return bookResponse;
    }

    public async Task<IEnumerable<Borrowing>> GetBorrowings(CancellationToken cancellationToken = default)
    {
        var bookResponse = await borrowingHttpClient.GetFromJsonAsync<IEnumerable<Borrowing>>("/api/cqrs/v1/borrowings", cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve borrowers from the borrower service.");
        return bookResponse;
    }

    public async Task<IEnumerable<BorrowingHistoryItem>> GetBorrowingHistoryItems(Guid? borrowerId, Guid? bookId, string query = "", CancellationToken cancellationToken = default)
    {
        var queryString = new StringBuilder();
        if (borrowerId.HasValue)
        {
            queryString.Append($"borrowerId={borrowerId.Value}&");
        }
        if (bookId.HasValue)
        {
            queryString.Append($"bookId={bookId.Value}&");
        }
        if (!string.IsNullOrWhiteSpace(query))
        {
            queryString.Append($"query={Uri.EscapeDataString(query)}&");
        }

        var bookResponse = await borrowingHistoryHttpClient.GetFromJsonAsync<IEnumerable<BorrowingHistoryItem>>($"/api/cqrs/v1/history/items?{queryString}", cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve borrowing history from the borrowing history service.");
        return bookResponse;
    }
}
