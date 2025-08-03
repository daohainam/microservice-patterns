
namespace Mcp.CQRS.Library.McpServer;
public class LibraryService(HttpClient bookHttpClient) : ILibraryService
{
    public async Task<IEnumerable<Book>> GetBooks(CancellationToken cancellationToken = default)
    {
        var bookResponse = await bookHttpClient.GetFromJsonAsync<IEnumerable<Book>>("/api/cqrs/v1/books", cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve books from the book service.");
        return bookResponse;
    }
}
