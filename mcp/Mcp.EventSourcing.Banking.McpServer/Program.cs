using Mcp.EventSourcing.Banking.McpServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(AccountTool).Assembly);

builder.Services.AddHttpClient("account",
    static client => client.BaseAddress = new("https+http://EventSourcing-Banking-AccountService"));

builder.Services.AddSingleton<IAccountService>((services) => new AccountService(
    services.GetRequiredService<IHttpClientFactory>().CreateClient("account")
    ));

var app = builder.Build();

app.MapMcp();
app.MapDefaultEndpoints();

app.Run();
