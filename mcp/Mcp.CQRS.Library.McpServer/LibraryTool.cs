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

}
