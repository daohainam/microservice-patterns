using Mcp.CQRS.Library.McpServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(LibraryTool).Assembly);

builder.Services.AddHttpClient("book",
    static client => client.BaseAddress = new("https+http://CQRS-Library-BookService"));
builder.Services.AddHttpClient("borrower",
    static client => client.BaseAddress = new("https+http://CQRS-Library-BorrowerService"));
builder.Services.AddHttpClient("borrower",
    static client => client.BaseAddress = new("https+http://CQRS-Library-BorrowingService"));
builder.Services.AddHttpClient("borrower",
    static client => client.BaseAddress = new("https+http://CQRS-Library-BorrowingHistoryService"));

builder.Services.AddSingleton<ILibraryService>((services) => new LibraryService(
    services.GetRequiredService<IHttpClientFactory>().CreateClient("book"),
    services.GetRequiredService<IHttpClientFactory>().CreateClient("borrower"),
    services.GetRequiredService<IHttpClientFactory>().CreateClient("borrowing"),
    services.GetRequiredService<IHttpClientFactory>().CreateClient("borrowing-history")
    ));

// Add services to the container.

var app = builder.Build();

app.MapMcp();
app.MapDefaultEndpoints();

//app.UseHttpsRedirection();

app.Run();
