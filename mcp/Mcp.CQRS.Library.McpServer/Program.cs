using Mcp.CQRS.Library.McpServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(LibraryTool).Assembly);

builder.Services.AddHttpClient("book",
    static client => client.BaseAddress = new("https+http://CQRS-Library-BookService"));

builder.Services.AddSingleton<ILibraryService>((services) => new LibraryService(
    services.GetRequiredService<IHttpClientFactory>().CreateClient("book")
    ));

// Add services to the container.

var app = builder.Build();

app.MapMcp();
app.MapDefaultEndpoints();

//app.UseHttpsRedirection();

app.Run();
