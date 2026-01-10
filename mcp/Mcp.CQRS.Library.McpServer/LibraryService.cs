
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

    public async Task<Book> CreateBook(Book book, CancellationToken cancellationToken = default)
    {
        var response = await bookHttpClient.PostAsJsonAsync("/api/cqrs/v1/books", book, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        var createdBook = await response.Content.ReadFromJsonAsync<Book>(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to create book.");
        return createdBook;
    }

    public async Task UpdateBook(Guid bookId, Book book, CancellationToken cancellationToken = default)
    {
        var response = await bookHttpClient.PutAsJsonAsync($"/api/cqrs/v1/books/{bookId}", book, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Borrower> CreateBorrower(Borrower borrower, CancellationToken cancellationToken = default)
    {
        var response = await borrowerHttpClient.PostAsJsonAsync("/api/cqrs/v1/borrowers", borrower, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        var createdBorrower = await response.Content.ReadFromJsonAsync<Borrower>(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to create borrower.");
        return createdBorrower;
    }

    public async Task UpdateBorrower(Guid borrowerId, Borrower borrower, CancellationToken cancellationToken = default)
    {
        var response = await borrowerHttpClient.PutAsJsonAsync($"/api/cqrs/v1/borrowers/{borrowerId}", borrower, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Borrowing> CreateBorrowing(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        var response = await borrowingHttpClient.PostAsJsonAsync("/api/cqrs/v1/borrowings", borrowing, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
        var createdBorrowing = await response.Content.ReadFromJsonAsync<Borrowing>(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to create borrowing.");
        return createdBorrowing;
    }

    public async Task ReturnBook(Guid borrowingId, CancellationToken cancellationToken = default)
    {
        var response = await borrowingHttpClient.PutAsync($"/api/cqrs/v1/borrowings/{borrowingId}/return", null, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
