namespace Mcp.CQRS.Library.McpServer;
public interface ILibraryService
{
    Task<IEnumerable<Book>> GetBooks(CancellationToken cancellationToken = default);
}
