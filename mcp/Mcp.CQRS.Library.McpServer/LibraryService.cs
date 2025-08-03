
namespace Mcp.CQRS.Library.McpServer;
public class LibraryService(HttpClient bookHttpClient, HttpClient borrowerHttpClient) : ILibraryService
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
}
