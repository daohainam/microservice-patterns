using ModelContextProtocol.Server;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mcp.CQRS.Library.McpServer;

[McpServerToolType]
public class LibraryTool
{
    [McpServerTool]
    public static async Task<string> GetBooks(ILibraryService libraryService, CancellationToken cancellationToken)
    {
        var books = await libraryService.GetBooks(cancellationToken);

        return JsonSerializer.Serialize(books);
    }

    [McpServerTool]
    public static async Task<string> GetBookDetails(ILibraryService libraryService, string bookId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(bookId, out var parsedBookId))
        {
            throw new ArgumentException($"Invalid book ID format: '{bookId}'. Expected a valid GUID.", nameof(bookId));
        }

        var book = await libraryService.GetBookById(parsedBookId, cancellationToken) ?? throw new KeyNotFoundException($"Book with ID '{bookId}' not found.");
        return JsonSerializer.Serialize(book);
    }

    [McpServerTool]
    public static async Task<string> GetBorrowers(ILibraryService libraryService, CancellationToken cancellationToken)
    {
        var borrowers = await libraryService.GetBorrowers(cancellationToken);
        return JsonSerializer.Serialize(borrowers);
    }

    [McpServerTool]
    public static async Task<string> GetBorrowerDetails(ILibraryService libraryService, string borrowerId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(borrowerId, out var parsedBorrowerId))
        {
            throw new ArgumentException($"Invalid borrower ID format: '{borrowerId}'. Expected a valid GUID.", nameof(borrowerId));
        }
        var borrower = await libraryService.GetBorrowerById(parsedBorrowerId, cancellationToken) ?? throw new KeyNotFoundException($"Borrower with ID '{borrowerId}' not found.");
        return JsonSerializer.Serialize(borrower);
    }

    [McpServerTool]
    public static async Task<string> GetBorrowings(ILibraryService libraryService, CancellationToken cancellationToken)
    {
        var borrowings = await libraryService.GetBorrowings(cancellationToken);
        return JsonSerializer.Serialize(borrowings);
    }

    [McpServerTool]
    public static async Task<string> GetBorrowingDetails(ILibraryService libraryService, string borrowingId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(borrowingId, out var parsedBorrowingId))
        {
            throw new ArgumentException($"Invalid borrowing ID format: '{borrowingId}'. Expected a valid GUID.", nameof(borrowingId));
        }
        var borrowing = await libraryService.GetBorrowingById(parsedBorrowingId, cancellationToken) ?? throw new KeyNotFoundException($"Borrowing with ID '{borrowingId}' not found.");
        return JsonSerializer.Serialize(borrowing);
    }


}
